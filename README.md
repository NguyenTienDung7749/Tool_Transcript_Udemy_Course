# 🎬 Udemy Transcript Extractor

A stunning, ultra-modern WPF application for extracting transcripts from Udemy courses with a premium dark theme inspired by Spotify, Discord, and Notion.

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## ✨ Features

### 🎨 Premium UI/UX
- **Dark Mode Theme**: Deep dark backgrounds (#0D1117) with vibrant accent colors
- **Glassmorphism Effects**: Modern blur and transparency effects
- **Smooth Animations**: Buttery 60fps transitions throughout the app
- **Micro-interactions**: Hover effects, scale animations, and ripples
- **Custom Window Chrome**: Modern window controls (no default Windows style)

### 🚀 Core Functionality
- **Integrated WebView2 Browser**: Browse Udemy directly within the app
- **One-Click Extraction**: Floating action button for easy transcript extraction
- **Auto-Save**: Automatically saves transcripts to your preferred folder
- **Recent Files**: Quick access to your last 5 extracted transcripts
- **Real-time Stats**: Track your extraction progress

### 🎭 Advanced Features
- **Toast Notifications**: Elegant slide-in notifications for all actions
- **Confetti Effects**: Celebratory animations on successful extractions
- **Success Animations**: Checkmark transformations with spring effects
- **Easter Egg**: Konami code (↑↑↓↓←→←→BA) triggers party mode 🎉
- **Responsive Design**: Adapts to window resizing (min 960x600)

## 🛠️ Tech Stack

### Core Technologies
- **.NET 8.0** - Latest .NET framework with Windows targeting
- **WPF (Windows Presentation Foundation)** - Modern Windows desktop UI
- **WebView2** - Microsoft Edge-based web browser control

### NuGet Packages
- `Microsoft.Web.WebView2` (1.0.2592.51) - Embedded browser
- `ModernWpfUI` (0.9.6) - Modern UI components
- `MaterialDesignThemes` (4.9.0) - Material Design icons and styles
- `CommunityToolkit.Mvvm` (8.2.2) - MVVM pattern helpers
- `Microsoft.Xaml.Behaviors.Wpf` (1.1.77) - XAML behaviors
- `System.Text.Json` (8.0.5) - Settings persistence

## 🏗️ Architecture

### MVVM Pattern
The application follows the Model-View-ViewModel (MVVM) pattern:
- **Models**: Data structures (AppSettings, ExtractedFile)
- **ViewModels**: Business logic and data binding (MainViewModel)
- **Views**: XAML UI components (MainWindow, Controls)

### Project Structure
```
UdemyTranscriptExtractor/
├── Assets/Icons/           # Application icons and resources
├── Themes/                 # XAML resource dictionaries
│   ├── Colors.xaml        # Color palette and brushes
│   ├── Buttons.xaml       # Button styles and templates
│   ├── TextStyles.xaml    # Typography styles
│   ├── Cards.xaml         # Card and container styles
│   └── Animations.xaml    # Animation storyboards
├── ViewModels/            # MVVM ViewModels
│   └── MainViewModel.cs   # Main window view model
├── Views/                 # XAML views (currently MainWindow)
├── Controls/              # Custom user controls
│   ├── FloatingActionButton.xaml/cs
│   └── ToastNotification.xaml/cs
├── Services/              # Business logic services
│   ├── TranscriptService.cs
│   ├── SettingsService.cs
│   └── NotificationService.cs
├── Helpers/               # Utility classes
│   ├── AnimationHelper.cs
│   ├── ConfettiEffect.cs
│   └── EasterEggHandler.cs
├── Models/                # Data models
│   ├── ExtractedFile.cs
│   └── AppSettings.cs
├── App.xaml/cs           # Application entry point
└── MainWindow.xaml/cs    # Main application window
```

## 🎨 Design System

### Color Palette
```
Primary Background:   #0D1117 (Deep dark)
Secondary Background: #161B22 (Card background)
Tertiary Background:  #21262D (Hover states)

Accent Blue:          #58A6FF (Primary accent)
Accent Purple:        #7C3AED (Secondary accent)
Success Green:        #3FB950
Error Red:            #F85149
Warning Orange:       #D29922

Text Primary:         #F0F6FC (Almost white)
Text Secondary:       #8B949E (Muted gray)
Border:               #30363D (Subtle borders)
```

### Typography
- **Title**: 24px Bold (Segoe UI Variable Display)
- **Subtitle**: 16px SemiBold
- **Body**: 14px Regular
- **Caption**: 12px Regular
- **Code**: 13px Consolas

### Layout
- **Window Size**: 1280x800 (default), resizable
- **Min Size**: 960x600
- **Top Bar**: 60px height
- **Sidebar**: 280px width (collapsible)
- **Bottom Bar**: 50px height
- **Border Radius**: 12px for cards and containers

## 📦 Installation

### Prerequisites
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime or SDK
- WebView2 Runtime (usually pre-installed on Windows 11)

### Building from Source

1. **Clone the repository**
   ```bash
   git clone https://github.com/NguyenTienDung7749/Tool_Transcript_Udemy_Course.git
   cd Tool_Transcript_Udemy_Course
   ```

2. **Restore dependencies**
   ```bash
   cd UdemyTranscriptExtractor
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build --configuration Release
   ```

4. **Run the application**
   ```bash
   dotnet run --configuration Release
   ```

### Published Release
Coming soon - downloadable .exe installer

## 🎯 Usage

1. **Launch the Application**: Start UdemyTranscriptExtractor.exe
2. **Navigate to Udemy**: Use the built-in browser to visit your Udemy course
3. **Detect Transcript**: The app will automatically detect when a transcript is available
4. **Extract**: Click the floating action button (bottom-right) to extract
5. **Save**: Transcript is automatically saved to your chosen folder
6. **View Recent**: Check the sidebar for recently extracted files

### Settings
- Click the ⚙️ icon to configure output folder
- Recent files are automatically tracked (last 5)
- Settings are persisted in `%AppData%\UdemyTranscriptExtractor\settings.json`

## 🎬 Animations & Effects

### Button Animations
- Hover: Scale 1.02x, shadow increase (150ms)
- Click: Ripple effect
- FAB Pulse: Continuous 2s cycle when transcript detected

### Page Transitions
- Fade in: 300ms cubic-bezier(0.4, 0.0, 0.2, 1)
- Sidebar toggle: 250ms ease-in-out
- Toast notifications: 200ms slide + fade

### Success Celebration
- Checkmark: 400ms spring animation (elastic ease)
- Confetti: 50 particles with physics simulation
- Count-up: 500ms number animation

## 🐛 Known Limitations

- WebView2 requires internet connection for initial setup
- Transcript extraction is a demonstration (placeholder implementation)
- Windows-only (WPF limitation)
- Requires .NET 8.0 or higher

## 🔧 Development

### Running in Development
```bash
dotnet run
```

### Hot Reload
WPF supports XAML hot reload in Visual Studio 2022:
1. Run with debugger (F5)
2. Edit XAML files
3. Changes apply automatically

### Adding Dependencies
```bash
dotnet add package PackageName --version x.x.x
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

MIT License - see LICENSE file for details

## 🙏 Acknowledgments

- UI inspiration: Spotify, Discord, Notion, Arc Browser
- Icons: Material Design Icons
- Community: WPF community and contributors

---

**Made with ❤️ by dzung9f 🚀**
