# 🚀 Quick Start Guide

Get up and running with Udemy Transcript Extractor in minutes!

## Prerequisites

Before you begin, ensure you have:

✅ **Windows 10 or Windows 11** (64-bit)  
✅ **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)  
✅ **WebView2 Runtime** - Usually pre-installed on Windows 11, [Download here](https://developer.microsoft.com/microsoft-edge/webview2/) if needed

## 🏗️ Building from Source

### Step 1: Clone the Repository
```bash
git clone https://github.com/NguyenTienDung7749/Tool_Transcript_Udemy_Course.git
cd Tool_Transcript_Udemy_Course
```

### Step 2: Navigate to Project
```bash
cd UdemyTranscriptExtractor
```

### Step 3: Restore Dependencies
```bash
dotnet restore
```

This will download all required NuGet packages:
- Microsoft.Web.WebView2
- ModernWpfUI
- MaterialDesignThemes
- CommunityToolkit.Mvvm
- Microsoft.Xaml.Behaviors.Wpf
- System.Text.Json

### Step 4: Build the Project
```bash
dotnet build --configuration Release
```

### Step 5: Run the Application
```bash
dotnet run --configuration Release
```

Or navigate to the build output and run the executable:
```bash
cd bin/Release/net8.0-windows
./UdemyTranscriptExtractor.exe
```

## 🎮 Using the Application

### First Launch

1. **Welcome Screen**: The app will fade in with a smooth animation
2. **Browser Initialization**: WebView2 will load Udemy.com automatically
3. **Sidebar**: Check out the stats and recent files on the left

### Extracting Transcripts

1. **Navigate to Course**: 
   - Use the built-in browser to visit your Udemy course
   - Log in to your Udemy account if needed
   - Navigate to a lecture with transcripts

2. **Detection**:
   - The app automatically detects when transcripts are available
   - A green banner will slide down: "✓ Transcript Detected"

3. **Extract**:
   - Click the floating purple-blue button in the bottom-right
   - Watch the button transform to a checkmark
   - Confetti celebrates your success! 🎉

4. **Find Your File**:
   - Transcripts save to: `Documents/UdemyTranscripts/`
   - Check the bottom bar for the last saved file
   - View recent files in the sidebar

### Settings

Click the ⚙️ icon in the top bar to:
- Change output folder location
- Configure auto-save preferences
- View app information

### Recent Files

The sidebar shows your last 5 extracted transcripts:
- Hover over items to see the full path
- Click to open in your default text editor
- See extraction date and file size

## 🎨 UI Features

### Top Bar
- **📁 Folder Icon**: Select output folder
- **⚙️ Settings Icon**: Open settings
- **─ □ ×**: Custom window controls (minimize, maximize, close)

### Sidebar
- **📊 Dashboard**: View extraction statistics
- **📁 Recent Files**: Quick access to last 5 files
- **🚀 Branding**: Made by dzung9f

### Floating Action Button (FAB)
- **Purple-Blue Gradient**: Extract transcript
- **Pulsing Animation**: Ready to extract
- **Hover Effect**: Scales up and rotates
- **Success State**: Transforms to checkmark

### Toast Notifications
- Appear in top-right corner
- Auto-dismiss after 3 seconds
- Types: Success, Error, Info, Warning

## ⌨️ Keyboard Shortcuts

- **Drag Window**: Click and drag anywhere on the top bar
- **Easter Egg**: Press ↑↑↓↓←→←→BA for a surprise! 🎉

## 🎯 Tips & Tricks

### Maximize Your Experience

1. **Use Full Screen**: Maximize the window for the best browsing experience
2. **Recent Files**: Hover over items in the sidebar for full paths
3. **Quick Access**: Recent files are clickable (opens in default editor)
4. **Celebrations**: Watch for confetti on successful extractions

### Troubleshooting

#### WebView2 Not Loading
- Ensure WebView2 Runtime is installed
- Check internet connection for initial setup
- Restart the application

#### Transcript Not Detected
- Make sure you're on a Udemy lecture page
- Ensure the lecture has transcripts enabled
- Try refreshing the page in the browser

#### Files Not Saving
- Check output folder permissions
- Verify disk space
- Review settings for correct output path

## 🔧 Development Mode

### Hot Reload (Visual Studio 2022)

1. Open `UdemyTranscriptExtractor.sln` in Visual Studio
2. Press F5 to run with debugger
3. Edit XAML files while running
4. Changes apply automatically!

### Debug Mode

```bash
dotnet run --configuration Debug
```

### Watch Mode

```bash
dotnet watch run
```

## 📦 Creating a Standalone Release

### Publish as Single File

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output location: `bin/Release/net8.0-windows/win-x64/publish/`

### Distribution

The published .exe can be:
- Shared directly (no installation needed)
- Packaged into an installer
- Distributed via Microsoft Store

## 🎨 Customization

### Change Colors

Edit `Themes/Colors.xaml` to customize the color scheme:

```xaml
<Color x:Key="AccentColor">#YOUR_COLOR</Color>
```

### Modify Layout

Edit `MainWindow.xaml` to change:
- Window size
- Sidebar width
- Component positions

### Add Features

The MVVM architecture makes it easy to extend:
1. Create new ViewModels in `ViewModels/`
2. Add new Views in `Views/`
3. Register services in `App.xaml.cs`

## 📚 Additional Resources

- **Full README**: See `README.md` for complete documentation
- **Design Guide**: See `DESIGN.md` for UI specifications
- **API Docs**: Coming soon
- **Video Tutorial**: Coming soon

## 🆘 Getting Help

### Common Issues

**Q: "Cannot find .NET 8.0"**  
A: Download and install .NET 8.0 SDK from Microsoft

**Q: "WebView2 runtime missing"**  
A: Download from Microsoft Edge WebView2 page

**Q: "Build errors"**  
A: Run `dotnet clean` then `dotnet restore`

**Q: "App won't start"**  
A: Check Windows version (needs Win10+)

### Support Channels

- **GitHub Issues**: Report bugs and request features
- **Discussions**: Ask questions and share ideas
- **Email**: Contact dzung9f for support

## 🎉 You're Ready!

Congratulations! You're now ready to extract Udemy transcripts with style.

**Enjoy the app and happy learning! 🚀**

---

Made with ❤️ by dzung9f

*Looking like $199 commercial software!* ✨
