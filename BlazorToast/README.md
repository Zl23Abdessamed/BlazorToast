# Blazor-Toast

A comprehensive and feature-rich toast notification library for Blazor applications with extensive customization options, animations, and accessibility support.

## Features

- ✨ **Multiple Toast Types**: Success, Error, Warning, Info, Loading, and Custom
- 🎨 **Rich Animations**: Fade, Slide, Bounce, Scale, Flip, Shake, Spin, Elastic, Zoom
- 🎯 **Flexible Positioning**: 6 different positions (TopRight, TopLeft, BottomRight, BottomLeft, TopCenter, BottomCenter)
- 🌙 **Theme Support**: Light, Dark, System, and Auto themes
- ⏱️ **Loading States**: Built-in loading toasts with progress tracking
- 🔄 **Promise Integration**: Automatic loading→success/error flow
- 🎭 **Custom Content**: Support for custom render fragments and icons
- 🎵 **Sound Support**: Play custom sounds on toast display
- ♿ **Accessibility**: Full ARIA support and screen reader compatibility
- 📱 **Responsive**: Works seamlessly across all device sizes
- 🎛️ **Global Configuration**: Centralized settings management
- 🔢 **Grouping & Limits**: Toast grouping and maximum display limits
- ⏸️ **Pause on Hover**: Automatic timer pause on mouse interaction
- 🎯 **Action Buttons**: Add custom action buttons to toasts

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Blazor-Toast
```

Or via Package Manager Console:

```powershell
Install-Package Blazor-Toast
```

## Setup

### 1. Register the Service

Add the service to your `Program.cs` or `Startup.cs`:

```csharp
// Program.cs (Blazor Server/WebAssembly)
using BlazorToast.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddBlazorToast();

// Or with global configuration
builder.Services.AddBlazorToast(config =>
{
    config.DefaultPosition = ToastPosition.TopCenter;
    config.DefaultDuration = 4000;
    config.MaxVisibleToasts = 5;
});

var app = builder.Build();
```

### 2. Include CSS and JavaScript

Add the following to your `_Host.cshtml` (Blazor Server) or `index.html` (Blazor WebAssembly):

```html
<!-- Required CSS -->
<link href="_content/Blazor-Toast/BlazorToast.css" rel="stylesheet" />

<!-- Optional: For dark mode detection and sound support -->
<script src="_content/Blazor-Toast/BlazorToastJsServices.js"></script>
```

### 3. Add the Component

Add the toast container to your main layout (e.g., `MainLayout.razor`):

```razor
@using BlazorToast

<div class="page">
    <!-- Your existing layout content -->
    
    <!-- Toast container - should be at the root level -->
    <BlazorToast />
</div>
```

## Basic Usage

### Inject the Service

```csharp
@inject BlazorToastService ToastService
```

### Simple Toast Messages

```csharp
// Success toast
await ToastService.ShowSuccess("Operation completed successfully!");

// Error toast
await ToastService.ShowError("Something went wrong!");

// Warning toast
await ToastService.ShowWarning("Please check your input.");

// Info toast
await ToastService.ShowInfo("New update available.");

// Loading toast
var loadingToast = await ToastService.ShowLoading("Processing...");
```

## Advanced Usage

### Using the Fluent Builder API

```csharp
// Complex toast with multiple options
var toast = Toast.Builder()
    .Message("File uploaded successfully!")
    .Success()
    .WithDuration(6000)
    .WithPosition(ToastPosition.TopCenter)
    .WithAnimation(ToastAnimation.SlideDown)
    .WithIcon("🎉")
    .WithAction("View", async () => NavigateToFile())
    .WithAction("Share", async () => ShareFile(), isPrimary: true)
    .WithSound("/sounds/success.mp3")
    .Build();

await ToastService.ShowToast(toast);
```

### Direct Toast Message Creation

```csharp
// Create toast with static methods
var successToast = ToastMessage.Success("Data saved!")
    .WithDuration(4000)
    .WithPosition(ToastPosition.TopRight)
    .WithAnimation(ToastAnimation.Bounce)
    .WithAction("Undo", UndoAction);

await ToastService.ShowToast(successToast);

// Error toast with custom styling
var errorToast = ToastMessage.Error("Validation failed!")
    .WithClass("my-custom-error")
    .WithStyle("border-left: 4px solid red;")
    .Persistent(); // Requires manual dismissal

await ToastService.ShowToast(errorToast);
```

### Loading States and Progress

```csharp
// Simple loading toast
var loadingToast = await ToastService.ShowLoadingToast("Uploading file...");

// Update progress
await loadingToast.UpdateProgress(0.5); // 50%

// Update message
await loadingToast.UpdateMessage("Processing file...");

// Complete with success
await loadingToast.Success("File uploaded successfully!");

// Or complete with error
// await loadingToast.Error("Upload failed!");
```

### Promise-Based Operations

```csharp
// Automatic loading → success/error flow
var result = await ToastService.ShowPromiseToast(
    promise: SaveDataAsync(),
    loadingMessage: () => "Saving data...",
    successMessage: result => $"Saved {result.Count} records",
    errorMessage: ex => $"Save failed: {ex.Message}"
);

// For void tasks
await ToastService.ShowPromiseToast(
    promise: DeleteFileAsync(),
    loadingMessage: () => "Deleting file...",
    successMessage: () => "File deleted successfully",
    errorMessage: ex => "Delete operation failed"
);
```

### Custom Content Toasts

```razor
@code {
    private async Task ShowCustomToast()
    {
        RenderFragment customContent = @<div>
            <h4>Custom Notification</h4>
            <p>This is a custom toast with HTML content.</p>
            <img src="/images/icon.png" alt="Icon" style="width: 24px; height: 24px;" />
        </div>;

        await ToastService.ShowCustom(customContent, duration: 8000);
    }
}
```

### Grouped Toasts

```csharp
// Add toasts to a group
await ToastService.ShowToast(
    ToastMessage.Info("First message").InGroup("notifications")
);

await ToastService.ShowToast(
    ToastMessage.Info("Second message").InGroup("notifications")
);

// Replace entire group
await ToastService.ShowToast(
    ToastMessage.Success("New message")
        .InGroup("notifications", replaceGroup: true)
);

// Dismiss entire group
await ToastService.DismissGroup("notifications");
```

## Animation Types

Choose from various built-in animations:

```csharp
// Available animations
ToastAnimation.Fade      // Default fade in/out
ToastAnimation.Slide     // Slide from right
ToastAnimation.SlideLeft // Slide from left
ToastAnimation.SlideUp   // Slide from bottom
ToastAnimation.SlideDown // Slide from top
ToastAnimation.Bounce    // Bounce effect
ToastAnimation.Scale     // Scale in/out
ToastAnimation.Flip      // 3D flip effect
ToastAnimation.Shake     // Shake animation
ToastAnimation.Spin      // Spin effect
ToastAnimation.Elastic   // Elastic bounce
ToastAnimation.Zoom      // Zoom in/out

// Usage
var toast = ToastMessage.Success("Animated!")
    .WithAnimation(ToastAnimation.Bounce);
```

## Global Configuration

Configure default behavior globally:

```csharp
ToastService.Configure(config =>
{
    config.DefaultPosition = ToastPosition.TopCenter;
    config.DefaultTheme = ToastTheme.Dark;
    config.DefaultAnimation = ToastAnimation.SlideDown;
    config.DefaultDuration = 5000;
    config.MaxVisibleToasts = 5;
    config.MaxToastsPerGroup = 3;
    config.CloseOnClick = true;
    config.PauseOnHover = true;
    config.ShowCloseButton = true;
    config.ShowHiddenToastCount = true;
    
    // Default icons for each type
    config.DefaultIcons[ToastType.Success] = "✅";
    config.DefaultIcons[ToastType.Error] = "❌";
    config.DefaultIcons[ToastType.Warning] = "⚠️";
    config.DefaultIcons[ToastType.Info] = "ℹ️";
    config.DefaultIcons[ToastType.Loading] = "🔄";
});
```

## Toast Management

```csharp
// Get toast information
var toastCount = ToastService.GetToastCount();
var errorCount = ToastService.GetToastCount(ToastType.Error);
var hasToast = ToastService.HasToast(toastId);
var toast = ToastService.GetToast(toastId);

// Dismiss specific toasts
await ToastService.DismissToast(toastId);

// Dismiss by type
await ToastService.DismissAll(ToastType.Error);

// Dismiss by group
await ToastService.DismissGroup("notifications");

// Clear all toasts
ToastService.ClearAll();

// Show hidden toasts
await ToastService.ShowAllHiddenToasts();
```

## Theming and Styling

### Built-in Themes

```csharp
// Theme options
ToastTheme.System  // Respects OS preference
ToastTheme.Light   // Light theme
ToastTheme.Dark    // Dark theme
ToastTheme.Auto    // Auto-detects based on time

// Apply theme
var toast = ToastMessage.Info("Themed message")
    .WithTheme(ToastTheme.Dark);
```

### Custom CSS Classes

```csharp
// Add custom CSS classes
var toast = ToastMessage.Success("Styled toast")
    .WithClass("my-success-toast custom-border")
    .WithStyle("box-shadow: 0 4px 12px rgba(0,0,0,0.15);");
```

### CSS Custom Properties

The library uses CSS custom properties for easy theming:

```css
:root {
    --zl-toast-success-bg: #10b981;
    --zl-toast-error-bg: #ef4444;
    --zl-toast-warning-bg: #f59e0b;
    --zl-toast-info-bg: #3b82f6;
    --zl-toast-border-radius: 8px;
    --zl-toast-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}
```

## Accessibility

The library provides comprehensive accessibility support:

```csharp
var accessibleToast = ToastMessage.Error("Form validation failed")
    .WithAccessibility(
        ariaLabel: "Form validation error",
        ariaDescription: "Please fix the highlighted fields",
        isImportant: true
    );
```

## Event Handling

```csharp
var toast = ToastMessage.Info("Interactive toast")
    .WithCallback(
        onClick: () => Console.WriteLine("Toast clicked!"),
        onDismiss: () => Console.WriteLine("Toast dismissed!")
    );

// Mouse events
toast.OnMouseEnter = () => Console.WriteLine("Mouse entered");
toast.OnMouseLeave = () => Console.WriteLine("Mouse left");

// Before dismiss validation
toast.BeforeDismiss = async () =>
{
    var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure?");
    return confirmed;
};
```

## API Reference

### ToastMessage Properties

| Property | Type | Description |
|----------|------|-------------|
| `Message` | `string` | The main toast message |
| `Type` | `ToastType` | Toast type (Success, Error, Warning, Info, Loading, Custom) |
| `Duration` | `int` | Auto-dismiss duration in milliseconds |
| `Position` | `ToastPosition` | Display position |
| `Theme` | `ToastTheme` | Visual theme |
| `Animation` | `ToastAnimation` | Enter/exit animation |
| `Icon` | `string` | Icon HTML or text |
| `CustomIcon` | `RenderFragment` | Custom icon component |
| `CustomContent` | `RenderFragment` | Custom toast content |
| `IsPersistent` | `bool` | Requires manual dismissal |
| `CloseOnClick` | `bool` | Close when clicked |
| `PauseOnHover` | `bool` | Pause timer on hover |
| `ShowCloseButton` | `bool` | Show close button |
| `Actions` | `List<ToastAction>` | Action buttons |
| `Progress` | `ToastProgress` | Progress bar settings |
| `GroupId` | `string` | Grouping identifier |
| `Priority` | `int` | Display priority |

### BlazorToastService Methods

| Method | Description |
|--------|-------------|
| `ShowToast(ToastMessage)` | Display a toast |
| `ShowSuccess(string, int?)` | Show success toast |
| `ShowError(string, int?)` | Show error toast |
| `ShowWarning(string, int?)` | Show warning toast |
| `ShowInfo(string, int?)` | Show info toast |
| `ShowLoading(string, int?)` | Show loading toast |
| `ShowCustom(RenderFragment, int?, ToastPosition?)` | Show custom content toast |
| `ShowPromiseToast<T>(Task<T>, ...)` | Show promise-based toast |
| `DismissToast(Guid)` | Dismiss specific toast |
| `DismissAll(ToastType?, string?)` | Dismiss multiple toasts |
| `ClearAll()` | Clear all toasts |

## Demo & Showcase

Check out the live demo and showcase of BlazorToast features:

- **Live Demo**: [https://zl23abdessamed.github.io/BlazorToastWebTest/](https://zl23abdessamed.github.io/BlazorToastWebTest/)

## GitHub Repository

The source code for BlazorToast is available on GitHub. You can explore the codebase, report issues, or contribute to the project:

- **Repository**: [https://github.com/Zl23Abdessamed/BlazorToast/](https://github.com/Zl23Abdessamed/BlazorToast/)
- **Issues**: Submit bugs or feature requests [here](https://github.com/Zl23Abdessamed/BlazorToast/issues)

## Contact

For questions, feedback, or support, you can reach out through the following channels:

- **Email**: [a_zalla@estin.dz](mailto:a_zalla@estin.dz)
- **GitHub Discussions**: Join the conversation on [BlazorToast Discussions](https://github.com/Zl23Abdessamed/BlazorToast/discussions)
- **Discord**: Join our community server at [https://discord.gg/84X9nm3fKn](https://discord.gg/84X9nm3fKn)

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

If you encounter any issues or have questions, please file an issue on the GitHub repository.