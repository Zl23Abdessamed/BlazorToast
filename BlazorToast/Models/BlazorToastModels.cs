using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorToast.Models
{
    public enum ToastType
    {
        Success,
        Error,
        Warning,
        Info,
        Loading,
        Custom
    }

    public enum ToastPosition
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft,
        TopCenter,
        BottomCenter
    }

    public enum ToastTheme
    {
        System, // Default - respects OS preference
        Light,
        Dark,
        Auto // Auto-detects based on time or system
    }

    public class ToastAnimation
    {
        public string EnterClass { get; set; } = "zl-fade-in";
        public string ExitClass { get; set; } = "zl-fade-out";
        public int Duration { get; set; } = 300; // Animation duration in ms

        // Predefined animations
        public static ToastAnimation Fade => new ToastAnimation();
        public static ToastAnimation Slide => new ToastAnimation
        {
            EnterClass = "zl-slide-in-right",
            ExitClass = "zl-slide-out-right"
        };
        public static ToastAnimation SlideLeft => new ToastAnimation
        {
            EnterClass = "zl-slide-in-left",
            ExitClass = "zl-slide-out-left"
        };
        public static ToastAnimation SlideUp => new ToastAnimation
        {
            EnterClass = "zl-slide-in-up",
            ExitClass = "zl-slide-out-up"
        };
        public static ToastAnimation SlideDown => new ToastAnimation
        {
            EnterClass = "zl-slide-in-down",
            ExitClass = "zl-slide-out-down"
        };
        public static ToastAnimation Bounce => new ToastAnimation
        {
            EnterClass = "zl-bounce-in",
            ExitClass = "zl-bounce-out",
            Duration = 500
        };
        public static ToastAnimation Scale => new ToastAnimation
        {
            EnterClass = "zl-scale-in",
            ExitClass = "zl-scale-out"
        };
        public static ToastAnimation Flip => new ToastAnimation
        {
            EnterClass = "zl-flip-in",
            ExitClass = "zl-flip-out",
            Duration = 400
        };
        public static ToastAnimation Shake => new ToastAnimation
        {
            EnterClass = "zl-shake",
            ExitClass = "zl-fade-out"
        };
        public static ToastAnimation Spin => new ToastAnimation
        {
            EnterClass = "zl-spin-in",
            ExitClass = "zl-spin-out",
            Duration = 400
        };
        public static ToastAnimation Elastic => new ToastAnimation
        {
            EnterClass = "zl-elastic-in",
            ExitClass = "zl-elastic-out",
            Duration = 600
        };
        public static ToastAnimation Zoom => new ToastAnimation
        {
            EnterClass = "zl-zoom-in",
            ExitClass = "zl-zoom-out"
        };
    }

    public class ToastAction
    {
        public string Label { get; set; } = string.Empty;
        public Func<Task> Action { get; set; } = () => Task.CompletedTask;
        public string? CssClass { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool CloseToastAfterAction { get; set; } = true;
    }

    public class ToastProgress
    {
        public bool Show { get; set; } = false;
        public double Value { get; set; } = 0; // 0-1
        public string? Color { get; set; }
        public bool Animated { get; set; } = true;
    }

    public class ToastMessage
    {
        public ToastMessage()
        {

            Id = Guid.NewGuid();
            Duration = 5000; // Default 5 seconds
            Position = ToastPosition.TopRight;
            Type = ToastType.Info;
            Theme = ToastTheme.System;
            Animation = ToastAnimation.Fade;
            Actions = new List<ToastAction>();
            Progress = new ToastProgress();
            CreatedAt = DateTime.UtcNow;

        }

        // Core properties
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; }
        public int Duration { get; set; }
        public ToastPosition Position { get; set; }
        public ToastTheme Theme { get; set; }
        public ToastAnimation Animation { get; set; }
        public DateTime CreatedAt { get; set; }

        // Visual customization
        public string? Icon { get; set; }
        public string? CustomClass { get; set; }
        public string? CustomStyle { get; set; }
        public RenderFragment? CustomContent { get; set; }
        public RenderFragment? CustomIcon { get; set; }

        // Behavior
        public bool IsPersistent { get; set; } = false; // Requires manual dismissal
        public bool CloseOnClick { get; set; } = true;
        public bool PauseOnHover { get; set; } = true;
        public bool ShowCloseButton { get; set; } = true;
        public bool IsLoading => Type == ToastType.Loading;

        // Grouping and stacking
        public string? GroupId { get; set; }
        public bool ReplaceGroup { get; set; } = false;
        public int Priority { get; set; } = 0; // Higher priority shows first

        // Interactions and callbacks
        public List<ToastAction> Actions { get; set; }
        public Action? OnClick { get; set; }
        public Action? OnDismiss { get; set; }
        public Action? OnShow { get; set; }
        public Action? OnMouseEnter { get; set; }
        public Action? OnMouseLeave { get; set; }
        public Func<Task<bool>>? BeforeDismiss { get; set; }

        // Progress and loading
        public ToastProgress Progress { get; set; }

        // Accessibility
        public string? AriaLabel { get; set; }
        public string? AriaDescription { get; set; }
        public bool IsImportant { get; set; } = false; // For screen readers
        public string? Role { get; set; } = "alert";

        // Audio
        public string? SoundUrl { get; set; }
        public int Delay { get; set; } = 0;
        // Helper factory methods
        public static ToastMessage Success(string message, int? duration = null)
            => Create(message, ToastType.Success, duration);

        public static ToastMessage Error(string message, int? duration = null)
            => Create(message, ToastType.Error, duration ?? 8000); // Errors stay longer

        public static ToastMessage Warning(string message, int? duration = null)
            => Create(message, ToastType.Warning, duration ?? 6000);

        public static ToastMessage Info(string message, int? duration = null)
            => Create(message, ToastType.Info, duration);

        public static ToastMessage Loading(string message, int? duration = null)
            => Create(message, ToastType.Loading, duration ?? 0); // Loading toasts don't auto-dismiss

        public static ToastMessage Custom(RenderFragment content, int? duration = null)
        {
            return new ToastMessage
            {
                CustomContent = content,
                Duration = duration ?? 5000,
                Type = ToastType.Custom
            };
        }

        public static ToastMessage Promise(string loadingMessage)
        {
            return new ToastMessage
            {
                Message = loadingMessage,
                Type = ToastType.Loading,
                Duration = 0,
                ShowCloseButton = false
            };
        }

        private static ToastMessage Create(string message, ToastType type, int? duration)
        {
            return new ToastMessage
            {
                Message = message,
                Type = type,
                Duration = duration ?? (type == ToastType.Loading ? 0 : 5000)
            };
        }

        // Fluent API methods
        public ToastMessage WithDuration(int milliseconds)
        {
            Duration = milliseconds;
            return this;
        }

        public ToastMessage WithPosition(ToastPosition position)
        {
            Position = position;
            return this;
        }

        public ToastMessage WithAnimation(ToastAnimation animation)
        {
            Animation = animation;
            return this;
        }

        public ToastMessage WithTheme(ToastTheme theme)
        {
            Theme = theme;
            return this;
        }

        public ToastMessage WithIcon(string icon)
        {
            Icon = icon;
            return this;
        }

        public ToastMessage WithCustomIcon(RenderFragment icon)
        {
            CustomIcon = icon;
            return this;
        }

        public ToastMessage WithClass(string cssClass)
        {
            CustomClass = cssClass;
            return this;
        }

        public ToastMessage WithStyle(string style)
        {
            CustomStyle = style;
            return this;
        }

        public ToastMessage WithSound(string soundUrl)
        {
            SoundUrl = soundUrl;
            return this;
        }

        public ToastMessage WithDelay(int milliseconds)
        {
            Delay = milliseconds;
            return this;
        }

        public ToastMessage Persistent()
        {
            IsPersistent = true;
            Duration = 0;
            return this;
        }

        public ToastMessage WithAction(string label, Func<Task> action, bool isPrimary = false)
        {
            Actions.Add(new ToastAction
            {
                Label = label,
                Action = action,
                IsPrimary = isPrimary
            });
            return this;
        }

        public ToastMessage WithActions(params ToastAction[] actions)
        {
            Actions.AddRange(actions);
            return this;
        }

        public ToastMessage WithProgress(double value = 0, bool show = true)
        {
            Progress.Show = show;
            Progress.Value = value;
            return this;
        }

        public ToastMessage WithCallback(Action? onClick = null, Action? onDismiss = null)
        {
            OnClick = onClick;
            OnDismiss = onDismiss;
            return this;
        }

        public ToastMessage InGroup(string groupId, bool replaceGroup = false)
        {
            GroupId = groupId;
            ReplaceGroup = replaceGroup;
            return this;
        }

        public ToastMessage WithPriority(int priority)
        {
            Priority = priority;
            return this;
        }

        public ToastMessage WithAccessibility(string ariaLabel, string? ariaDescription = null, bool isImportant = false)
        {
            AriaLabel = ariaLabel;
            AriaDescription = ariaDescription;
            IsImportant = isImportant;
            return this;
        }
    }

    // Builder pattern for fluent API
    public static class Toast
    {
        public static ToastBuilder Builder() => new ToastBuilder();
        public static ToastMessage Success(string message) => ToastMessage.Success(message);
        public static ToastMessage Error(string message) => ToastMessage.Error(message);
        public static ToastMessage Warning(string message) => ToastMessage.Warning(message);
        public static ToastMessage Info(string message) => ToastMessage.Info(message);
        public static ToastMessage Loading(string message) => ToastMessage.Loading(message);
        public static ToastMessage Custom(RenderFragment content) => ToastMessage.Custom(content);
    }

    public class ToastBuilder
    {
        private ToastMessage _toast = new();

        public ToastBuilder Message(string message)
        {
            _toast.Message = message;
            return this;
        }

        public ToastBuilder Success()
        {
            _toast.Type = ToastType.Success;
            return this;
        }

        public ToastBuilder Error()
        {
            _toast.Type = ToastType.Error;
            _toast.Duration = 8000; // Errors stay longer
            return this;
        }

        public ToastBuilder Warning()
        {
            _toast.Type = ToastType.Warning;
            _toast.Duration = 6000;
            return this;
        }

        public ToastBuilder Info()
        {
            _toast.Type = ToastType.Info;
            return this;
        }

        public ToastBuilder Loading()
        {
            _toast.Type = ToastType.Loading;
            _toast.Duration = 0;
            _toast.ShowCloseButton = false;
            return this;
        }

        public ToastBuilder WithAnimation(ToastAnimation animation)
        {
            _toast.Animation = animation;
            return this;
        }

        public ToastBuilder Position(ToastPosition position)
        {
            _toast.Position = position;
            return this;
        }

        public ToastBuilder Duration(int ms)
        {
            _toast.Duration = ms;
            return this;
        }

        public ToastBuilder Theme(ToastTheme theme)
        {
            _toast.Theme = theme;
            return this;
        }

        public ToastBuilder Icon(string icon)
        {
            _toast.Icon = icon;
            return this;
        }

        public ToastBuilder CustomIcon(RenderFragment icon)
        {
            _toast.CustomIcon = icon;
            return this;
        }

        public ToastBuilder Class(string cssClass)
        {
            _toast.CustomClass = cssClass;
            return this;
        }

        public ToastBuilder Style(string style)
        {
            _toast.CustomStyle = style;
            return this;
        }

        public ToastBuilder Persistent()
        {
            _toast.IsPersistent = true;
            _toast.Duration = 0;
            return this;
        }

        public ToastBuilder Action(string label, Func<Task> action, bool isPrimary = false)
        {
            _toast.Actions.Add(new ToastAction
            {
                Label = label,
                Action = action,
                IsPrimary = isPrimary
            });
            return this;
        }

        public ToastBuilder WithSound(string soundUrl)
        {
            _toast.SoundUrl = soundUrl;
            return this;
        }

        public ToastBuilder WithDelay(int milliseconds)
        {
            _toast.Delay = milliseconds;
            return this;
        }

        public ToastBuilder Progress(double value = 0, bool show = true)
        {
            _toast.Progress.Show = show;
            _toast.Progress.Value = value;
            return this;
        }

        public ToastBuilder OnClick(Action onClick)
        {
            _toast.OnClick = onClick;
            return this;
        }

        public ToastBuilder OnDismiss(Action onDismiss)
        {
            _toast.OnDismiss = onDismiss;
            return this;
        }

        public ToastBuilder InGroup(string groupId, bool replaceGroup = false)
        {
            _toast.GroupId = groupId;
            _toast.ReplaceGroup = replaceGroup;
            return this;
        }

        public ToastBuilder Priority(int priority)
        {
            _toast.Priority = priority;
            return this;
        }

        public ToastBuilder Accessibility(string ariaLabel, string? ariaDescription = null, bool isImportant = false)
        {
            _toast.AriaLabel = ariaLabel;
            _toast.AriaDescription = ariaDescription;
            _toast.IsImportant = isImportant;
            return this;
        }

        public ToastMessage Build() => _toast;
    }

    // Global configuration
    public class ToastGlobalConfig
    {
        public ToastPosition DefaultPosition { get; set; } = ToastPosition.TopRight;
        public ToastTheme DefaultTheme { get; set; } = ToastTheme.System;
        public ToastAnimation DefaultAnimation { get; set; } = ToastAnimation.Fade;
        public int DefaultDuration { get; set; } = 1000;
        public bool CloseOnClick { get; set; } = true;
        public bool PauseOnHover { get; set; } = true;
        public bool ShowCloseButton { get; set; } = true;
        public int MaxVisibleToasts { get; set; } = 5;
        public int MaxToastsPerGroup { get; set; } = 3;
        public string? ContainerClassName { get; set; }
        public string? ContainerStyle { get; set; }
        public Dictionary<ToastType, string> DefaultIcons { get; set; } = new()
        {
            { ToastType.Success, "✓" },
            { ToastType.Error, "✕" },
            { ToastType.Warning, "⚠" },
            { ToastType.Info, "ℹ" },
            { ToastType.Loading, "⟳" }
        };
        public Dictionary<ToastType, string> DefaultSounds { get; set; } = new();
        public bool ShowHiddenToastCount { get; set; } = true;
        public string HiddenToastCountClass { get; set; } = "";
        public string HiddenToastCountStyle { get; set; } = "";
        public string HiddenToastCountTemplate { get; set; } = "+{0} more";

    }
}