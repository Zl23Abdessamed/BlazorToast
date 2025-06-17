using BlazorToast.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace BlazorToast.Services
{
    public class LoadingToast : IDisposable
    {
        private readonly BlazorToastService _service;
        private readonly Guid _id;
        private bool _disposed = false;

        internal LoadingToast(BlazorToastService service, Guid id)
        {
            _service = service;
            _id = id;
        }

        public async Task UpdateMessage(string newMessage)
        {
            if (_disposed) return;
            await _service.UpdateToastMessage(_id, newMessage);
        }

        public async Task UpdateProgress(double progress)
        {
            if (_disposed) return;
            await _service.UpdateToastProgress(_id, progress);
        }

        public async Task Success(string message)
        {
            if (_disposed) return;
            await _service.ConvertToastType(_id, ToastType.Success, message);
            _disposed = true;
        }

        public async Task Error(string message)
        {
            if (_disposed) return;
            await _service.ConvertToastType(_id, ToastType.Error, message);
            _disposed = true;
        }

        public async Task Warning(string message)
        {
            if (_disposed) return;
            await _service.ConvertToastType(_id, ToastType.Warning, message);
            _disposed = true;
        }

        public async Task Info(string message)
        {
            if (_disposed) return;
            await _service.ConvertToastType(_id, ToastType.Info, message);
            _disposed = true;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _ = _service.DismissToast(_id);
                _disposed = true;
            }
        }
    }

    public class BlazorToastService : IDisposable
    {
        private readonly List<ToastMessage> _toasts = new();
        private readonly Queue<ToastMessage> _queuedToasts = new();
        private readonly Dictionary<Guid, System.Threading.Timer> _timers = new();
        private readonly Dictionary<Guid, System.Timers.Timer> _delayTimers = new();
        public int HiddenToastCount => Math.Max(0, _toasts.Count + _queuedToasts.Count - Config.MaxVisibleToasts);

        public bool HasHiddenToasts => HiddenToastCount > 0;

        public event Action? OnToastsChanged;
        public ToastGlobalConfig Config { get; private set; } = new();

        public IReadOnlyList<ToastMessage> ActiveToasts => _toasts
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToList()
            .AsReadOnly();

        public IReadOnlyList<ToastMessage> VisibleToasts => _toasts
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Take(Config.MaxVisibleToasts)
            .ToList()
            .AsReadOnly();

        // Configuration
        public void Configure(Action<ToastGlobalConfig> configAction)
        {
            configAction(Config);
        }

        public void UpdateConfig(ToastGlobalConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // Core show methods
        public async Task<Guid> ShowToast(ToastMessage toast)
        {
            ApplyGlobalDefaults(toast);

            // Handle group replacement
            if (!string.IsNullOrEmpty(toast.GroupId) && toast.ReplaceGroup)
            {
                var groupToasts = _toasts.Where(t => t.GroupId == toast.GroupId).ToList();
                foreach (var groupToast in groupToasts)
                {
                    await DismissToastInternal(groupToast.Id);
                }
            }

            // Handle group limits
            if (!string.IsNullOrEmpty(toast.GroupId))
            {
                var groupToasts = _toasts.Where(t => t.GroupId == toast.GroupId).ToList();
                if (groupToasts.Count >= Config.MaxToastsPerGroup)
                {
                    var oldestInGroup = groupToasts.OrderBy(t => t.CreatedAt).First();
                    await DismissToastInternal(oldestInGroup.Id);
                }
            }

            // Handle max visible toasts
            if (_toasts.Count >= Config.MaxVisibleToasts)
            {
                _queuedToasts.Enqueue(toast);
                return toast.Id;
            }

            if (toast.Delay > 0)
            {
                var delayTimer = new System.Timers.Timer(toast.Delay);
                delayTimer.Elapsed += async (sender, e) =>
                {
                    delayTimer.Stop();
                    delayTimer.Dispose();
                    _delayTimers.Remove(toast.Id);
                    await AddToastInternal(toast);
                };
                _delayTimers[toast.Id] = delayTimer;
                delayTimer.Start();
                return toast.Id;
            }


            await AddToastInternal(toast);
            return toast.Id;
        }

        private async Task AddToastInternal(ToastMessage toast)
        {
            _toasts.Add(toast);

            // Trigger show callback
            toast.OnShow?.Invoke();

            // Start auto-dismiss timer
            if (toast.Duration > 0 && !toast.IsPersistent)
            {
                StartAutoDismissTimer(toast.Id, toast.Duration);
            }

            OnToastsChanged?.Invoke();
            await Task.CompletedTask;
        }

        // Promise-style operations
        public async Task<T> ShowPromiseToast<T>(
    Task<T> promise,
    Func<string> loadingMessage,
    Func<T, string>? successMessage = null,
    Func<Exception, string>? errorMessage = null,
    ToastPosition? position = null)
        {
            var loadingToast = ToastMessage.Loading(loadingMessage());
            if (position.HasValue) loadingToast.Position = position.Value;

            var loadingId = await ShowToast(loadingToast);

            try
            {
                var result = await promise;
                await DismissToastInternal(loadingId);

                if (successMessage != null)
                {
                    var successMsg = successMessage(result);
                    if (!string.IsNullOrEmpty(successMsg))
                    {
                        var successToast = ToastMessage.Success(successMsg);
                        if (position.HasValue) successToast.Position = position.Value;
                        await ShowToast(successToast);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await DismissToastInternal(loadingId);

                if (errorMessage != null)
                {
                    var errorMsg = errorMessage(ex);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        var errorToast = ToastMessage.Error(errorMsg);
                        if (position.HasValue) errorToast.Position = position.Value;
                        await ShowToast(errorToast);
                    }
                }
                else
                {
                    var errorMsg = $"Error: {ex.Message}";
                    var errorToast = ToastMessage.Error(errorMsg);
                    if (position.HasValue) errorToast.Position = position.Value;
                    await ShowToast(errorToast);
                }

                throw;
            }
        }

        public async Task ShowPromiseToast(
    Task promise,
    Func<string> loadingMessage,
    Func<string>? successMessage = null,
    Func<Exception, string>? errorMessage = null,
    ToastPosition? position = null)
        {
            var loadingToast = ToastMessage.Loading(loadingMessage());
            if (position.HasValue) loadingToast.Position = position.Value;

            var loadingId = await ShowToast(loadingToast);

            try
            {
                await promise;
                await DismissToastInternal(loadingId);

                if (successMessage != null)
                {
                    var successMsg = successMessage();
                    if (!string.IsNullOrEmpty(successMsg))
                    {
                        var successToast = ToastMessage.Success(successMsg);
                        if (position.HasValue) successToast.Position = position.Value;
                        await ShowToast(successToast);
                    }
                }
            }
            catch (Exception ex)
            {
                await DismissToastInternal(loadingId);

                if (errorMessage != null)
                {
                    var errorMsg = errorMessage(ex);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        var errorToast = ToastMessage.Error(errorMsg);
                        if (position.HasValue) errorToast.Position = position.Value;
                        await ShowToast(errorToast);
                    }
                }
                else
                {
                    var errorMsg = $"Error: {ex.Message}";
                    var errorToast = ToastMessage.Error(errorMsg);
                    if (position.HasValue) errorToast.Position = position.Value;
                    await ShowToast(errorToast);
                }

                throw;
            }
        }

        // Loading toast with control
        public async Task<LoadingToast> ShowLoadingToast(string message, ToastPosition? position = null)
        {
            var toast = ToastMessage.Loading(message);
            if (position.HasValue) toast.Position = position.Value;

            var id = await ShowToast(toast);
            return new LoadingToast(this, id);
        }

        // Batch operations
        public async Task ShowBatch(params ToastMessage[] toasts)
        {
            foreach (var toast in toasts)
            {
                await ShowToast(toast);
            }
        }

        public async Task ShowBatch(IEnumerable<ToastMessage> toasts)
        {
            foreach (var toast in toasts)
            {
                await ShowToast(toast);
            }
        }

        // Dismissal methods
        public async Task<bool> DismissToast(Guid id)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == id);
            if (toast == null) return false;

            // Check if dismissal should be prevented
            if (toast.BeforeDismiss != null)
            {
                try
                {
                    var shouldDismiss = await toast.BeforeDismiss();
                    if (!shouldDismiss) return false;
                }
                catch
                {
                    return false;
                }
            }

            return await DismissToastInternal(id);
        }

        private async Task<bool> DismissToastInternal(Guid id)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == id);
            if (toast == null) return false;

            _toasts.Remove(toast);
            // Clean up timer
            if (_timers.TryGetValue(id, out var timer))
            {
                timer.Dispose();
                _timers.Remove(id);
            }

            // Trigger dismiss callback
            toast.OnDismiss?.Invoke();

            // Process queued toasts
            if (_queuedToasts.Count > 0 && _toasts.Count < Config.MaxVisibleToasts)
            {
                var nextToast = _queuedToasts.Dequeue();
                await AddToastInternal(nextToast);
            }

            OnToastsChanged?.Invoke();
            return true;
        }

        public async Task DismissAll(ToastType? type = null, string? groupId = null)
        {
            var toRemove = _toasts
                .Where(t =>
                    (!type.HasValue || t.Type == type.Value) &&
                    (groupId == null || t.GroupId == groupId))
                .ToList();

            foreach (var toast in toRemove)
            {
                await DismissToastInternal(toast.Id);
            }
        }

        public async Task DismissGroup(string groupId)
        {
            await DismissAll(groupId: groupId);
        }

        public void ClearAll()
        {
            foreach (var toast in _toasts.ToList())
            {
                _ = DismissToastInternal(toast.Id);
            }

            _queuedToasts.Clear();

            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }
            _timers.Clear();
        }

        // Update methods for loading toasts
        internal async Task UpdateToastMessage(Guid id, string newMessage)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == id);
            if (toast != null)
            {
                toast.Message = newMessage;
                OnToastsChanged?.Invoke();
            }
            await Task.CompletedTask;
        }

        internal async Task UpdateToastProgress(Guid id, double progress)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == id);
            if (toast != null)
            {
                toast.Progress.Value = Math.Max(0, Math.Min(1, progress));
                toast.Progress.Show = true;
                OnToastsChanged?.Invoke();
            }
            await Task.CompletedTask;
        }

        internal async Task ConvertToastType(Guid id, ToastType newType, string newMessage)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == id);
            if (toast != null)
            {
                toast.Type = newType;
                toast.Message = newMessage;
                toast.ShowCloseButton = true;
                toast.Progress.Show = false;

                // Set appropriate duration
                toast.Duration = newType switch
                {
                    ToastType.Success => 4000,
                    ToastType.Error => 8000,
                    ToastType.Warning => 6000,
                    ToastType.Info => 5000,
                    _ => 5000
                };

                // Start auto-dismiss
                StartAutoDismissTimer(id, toast.Duration);
                OnToastsChanged?.Invoke();
            }
            await Task.CompletedTask;
        }

        // Convenience methods
        public async Task<Guid> ShowSuccess(string message, int? duration = null)
        {
            return await ShowToast(ToastMessage.Success(message, duration ?? Config.DefaultDuration));
        }

        public async Task<Guid> ShowError(string message, int? duration = null)
        {
            return await ShowToast(ToastMessage.Error(message, duration));
        }

        public async Task<Guid> ShowWarning(string message, int? duration = null)
        {
            return await ShowToast(ToastMessage.Warning(message, duration));
        }

        public async Task<Guid> ShowInfo(string message, int? duration = null)
        {
            return await ShowToast(ToastMessage.Info(message, duration ?? Config.DefaultDuration));
        }

        public async Task<LoadingToast> ShowLoading(string message, int? duration = null)
        {
            var toast = ToastMessage.Loading(message, duration);
            var id = await ShowToast(toast);
            return new LoadingToast(this, id);
        }

        public async Task<Guid> ShowCustom(RenderFragment content, int? duration = null, ToastPosition? position = null)
        {
            var toast = ToastMessage.Custom(content, duration ?? Config.DefaultDuration);
            if (position.HasValue) toast.Position = position.Value;
            return await ShowToast(toast);
        }

        // Timer management - FIXED VERSION
        private void StartAutoDismissTimer(Guid toastId, int duration)
        {
            var timer = new System.Threading.Timer(async _ =>
            {
                await DismissToastInternal(toastId);
            }, null, duration, Timeout.Infinite);
            _timers[toastId] = timer; // Store the timer for cleanup
        }

        public void PauseTimer(Guid toastId)
        {
            if (_timers.TryGetValue(toastId, out var timer))
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void ResumeTimer(Guid toastId)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast != null && _timers.TryGetValue(toastId, out var timer))
            {
                timer.Change(toast.Duration, Timeout.Infinite);
            }
        }

        // Apply global defaults
        private void ApplyGlobalDefaults(ToastMessage toast)
        {
            if (toast.Theme == ToastTheme.System)
                toast.Theme = Config.DefaultTheme;

            if (toast.Animation == null || toast.Animation == ToastAnimation.Fade)
                toast.Animation = Config.DefaultAnimation;

            if (toast.Duration == 5000) // Only override if it's the default
                toast.Duration = Config.DefaultDuration;

            if (toast.Position == ToastPosition.TopRight) // Only override if it's the default
                toast.Position = Config.DefaultPosition;

            if (string.IsNullOrEmpty(toast.Icon) && Config.DefaultIcons.TryGetValue(toast.Type, out var defaultIcon))
                toast.Icon = defaultIcon;

            // Apply global behavior settings
            if (toast.CloseOnClick != false) // Only if not explicitly set to false
                toast.CloseOnClick = Config.CloseOnClick;

            if (toast.PauseOnHover != false) // Only if not explicitly set to false
                toast.PauseOnHover = Config.PauseOnHover;

            if (toast.ShowCloseButton != false) // Only if not explicitly set to false
                toast.ShowCloseButton = Config.ShowCloseButton;
        }

        // Query methods
        public bool HasToast(Guid id)
        {
            return _toasts.Any(t => t.Id == id);
        }

        public ToastMessage? GetToast(Guid id)
        {
            return _toasts.FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<ToastMessage> GetToastsByGroup(string groupId)
        {
            return _toasts.Where(t => t.GroupId == groupId);
        }

        public IEnumerable<ToastMessage> GetToastsByType(ToastType type)
        {
            return _toasts.Where(t => t.Type == type);
        }

        public int GetToastCount(ToastType? type = null, string? groupId = null)
        {
            var query = _toasts.AsEnumerable();

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            if (!string.IsNullOrEmpty(groupId))
                query = query.Where(t => t.GroupId == groupId);

            return query.Count();
        }

        // Event handlers for mouse interactions
        public void OnToastMouseEnter(Guid toastId)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast != null)
            {
                toast.OnMouseEnter?.Invoke();

                if (toast.PauseOnHover)
                {
                    PauseTimer(toastId);
                }
            }
        }

        public void OnToastMouseLeave(Guid toastId)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast != null)
            {
                toast.OnMouseLeave?.Invoke();

                if (toast.PauseOnHover)
                {
                    ResumeTimer(toastId);
                }
            }
        }

        public async Task OnToastClick(Guid toastId)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast != null)
            {
                toast.OnClick?.Invoke();

                if (toast.CloseOnClick)
                {
                    await DismissToast(toastId);
                }
            }
        }

        public async Task ShowAllHiddenToasts()
        {
            // Process all queued toasts immediately
            while (_queuedToasts.Count > 0 && _toasts.Count < Config.MaxVisibleToasts * 2) // Allow temporary overflow
            {
                var nextToast = _queuedToasts.Dequeue();
                await AddToastInternal(nextToast);
            }
        }

        // Cleanup
        public void Dispose()
        {
            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }

            foreach (var timer in _delayTimers.Values)
            {
                timer.Dispose();
            }
            _delayTimers.Clear();
            _timers.Clear();
            _toasts.Clear();
            _queuedToasts.Clear();
        }
    }
}