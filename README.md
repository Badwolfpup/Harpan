# Harpan

A Klondike solitaire card game built with C# and WPF. "Harpan" is Swedish for "Solitaire".

## Technologies

- C#, WPF (XAML), .NET

## Features

- Classic Klondike solitaire rules
- Full 52-card deck with graphical card images
- Deck management and card dealing logic
- Drag-and-drop style card movement

## Project Structure

```
Harpan/
├── MainWindow.xaml/.cs     # Game UI and interaction
├── Spelkort.cs             # Card model (spelkort = playing card)
├── HanteraKortlek.cs       # Deck management (hantera kortlek = manage deck)
├── BindingProxy.cs         # WPF binding helper
└── Spelkort/               # Card images (52 cards + suits + backs)
```

## How to Run

Open `Harpan.sln` in Visual Studio and run the project.
