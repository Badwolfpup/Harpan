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
â”œâ”€â”€ MainWindow.xaml/.cs     # Game UI and interaction
â”œâ”€â”€ Spelkort.cs             # Card model (spelkort = playing card)
â”œâ”€â”€ HanteraKortlek.cs       # Deck management (hantera kortlek = manage deck)
â”œâ”€â”€ BindingProxy.cs         # WPF binding helper
â””â”€â”€ Spelkort/               # Card images (52 cards + suits + backs)
```

## How to Run

Open `Harpan.sln` in Visual Studio and run the project.
