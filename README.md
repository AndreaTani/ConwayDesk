# ConwayDesk: Conway's Game of Life (WPF)
ConwayDesk is a sleek, responsive desktop application for simulating Conway's Game of Life, a zero-player cellular automaton. Built with C# and WPF, this application provides a robust and engaging platform to observe complex, emergent patterns from simple rules.

## Features
### Classic Game of Life Logic: Implements the core rules of Conway's Game of Life:
- Underpopulation: Any live cell with fewer than two live neighbours dies.
- Survival/Stasis: Any live cell with two or three live neighbours lives on to the next generation.
- Overcrowding: Any live cell with more than three live neighbours dies.
- Reproduction: Any dead cell with exactly three live neighbours becomes a live cell.
### Performance-Optimized Simulation: 
The logic for calculating the next generation runs asynchronously and in parallel using Task.Run and Parallel.For, ensuring smooth performance even with larger grid sizes.
### Interactive Grid Drawing:
Easily toggle the state of individual cells to create custom patterns before starting the simulation.
Support for click-and-drag drawing to quickly "paint" initial configurations.
### Responsive UI/UX:
The game grid dynamically resizes to fit the application window, optimizing the display of cells.
A custom fullscreen mode is activated when the window is maximized, providing an immersive viewing experience (toggle by moving the mouse to the top edge).
### Intuitive Controls: Simple buttons for:
- Start/Stop the simulation loop.
- Reset the grid to an empty state.
- Random Seed to instantly populate the grid with a random initial pattern.
### MVVM Architecture:
Clean separation of concerns using the Model-View-ViewModel pattern, making the codebase maintainable and scalable.
### Visual Style: 
A focused, dark-themed interface using a custom palette of Dark Grey, Medium Grey, and a vibrant Orange for active cells.
##  Technology Stack
- Primary Language: C#
- Framework: .NET Framework / .NET Core (targeting Windows desktop)
- UI Framework: Windows Presentation Foundation (WPF)
- Architecture: Model-View-ViewModel (MVVM)
- Concurrency: System.Threading.Tasks for parallel processing (Parallel.For)
## How It Works
### The heart of the application is the GameViewModel. It orchestrates the entire simulation:
The grid is represented by an ObservableCollection<Cell>.
A DispatcherTimer controls the fixed-interval game loop.
On each "tick" of the timer, the ComputeNextStateInParallel() method runs. It uses parallelism to calculate the state of every cell in the next generation based on its current neighbours, making the update highly efficient.
The Generation counter is updated and displayed in the header.
All user interactions (Start, Stop, Reset, Seed) are managed via RelayCommand bindings, keeping the UI decoupled from the logic.
### UI & Rendering (MainWindow.xaml and MainWindow.xaml.cs)
The grid is rendered using a WrapPanel inside an ItemsControl, which efficiently displays the Cell objects from the ViewModel.
Cell size (CellSize) is calculated dynamically in the code-behind (MainWindow.xaml.cs) to ensure the grid perfectly fills the available space when the window is resized.
The Cell objects implement INotifyPropertyChanged to enable immediate UI updates (changing color) when their IsActive state changes.
## Contributions
Feel free to clone, contribute, and explore the fascinating complexity of the Game of Life!
