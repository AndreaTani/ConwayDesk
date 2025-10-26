using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ConwayDesk
{

    public partial class MainWindow : Window
    {
        // ---------------------------------------------------------------------
        //                             FIELDS
        // ---------------------------------------------------------------------

        private const double FixedHeaderHeight = 80;
        private const double UITitleBarApproxHeight = 32;
        private const double HorizontalMarginCompensation = 20;
        private const double TotalVerticalMargins = 20;
        private bool _isDrawingMode = false;
        private Cell _lastClickedCell = null;
        private const double MouseDetectionArea = 10;


        // ---------------------------------------------------------------------
        //                           PROPERTIES
        // ---------------------------------------------------------------------

        private readonly GameViewModel _viewModel = new GameViewModel();


        // ---------------------------------------------------------------------
        //                          WINDOW LOGIC
        // ---------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateHeaderAndGameAreaSize();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameViewModel.IsGameRunning))
            {
                GameArea.IsHitTestVisible = !_viewModel.IsGameRunning;
            }
        }

        public int GridSize => _viewModel.GridSize;

        private void Cell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsGameRunning)
            {
                return;
            }

            if (!(sender is FrameworkElement element) || !(element.DataContext is Cell cell))
            {
                return;
            }

            _isDrawingMode = !cell.IsActive;

            cell.IsActive = _isDrawingMode;
            _lastClickedCell = cell;

            GameArea.CaptureMouse();

            e.Handled = true;
        }

        private void UpdateHeaderAndGameAreaSize()
        {
            double nonClientAreaHeight = 0;
            if (WindowStyle == WindowStyle.SingleBorderWindow)
            {
                nonClientAreaHeight = UITitleBarApproxHeight;
            }

            double availableWidth = ActualWidth - HorizontalMarginCompensation;
            double availableHeight = ActualHeight - FixedHeaderHeight - nonClientAreaHeight - TotalVerticalMargins;
            double gameAreaSize = Math.Max(0, Math.Min(availableWidth, availableHeight));

            if (GridSize > 0)
            {
                _viewModel.CellSize = gameAreaSize / GridSize;
            }
            else
            {
                _viewModel.CellSize = 0;
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                GameArea.Width = gameAreaSize;
                GameArea.Height = gameAreaSize;
                Header.Width = gameAreaSize;

            }), DispatcherPriority.Background);
        }

        // Garantisce l'aspect ratio della GameArea 1:1 al cambiamento della dimensione della finestra
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged || e.HeightChanged)
            {
                UpdateHeaderAndGameAreaSize();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateHeaderAndGameAreaSize();
        }

        // Gestisce la visualizzazione o meno della barra UI di wondows al
        // movimento del mouse, se il mouse si trova nel raggio di azione
        // della barra superiore, la barra torna visibile
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point position;

            if (e.LeftButton == MouseButtonState.Pressed && GameArea.IsMouseCaptured)
            {
                // Ottengo la posizione del mouse relativa alla GameArea
                position = e.GetPosition(GameArea);

                // Calcolo l'indice della riga e colonna basandomi sulla posizione e CellSize
                int col = (int)(position.X / _viewModel.CellSize);
                int row = (int)(position.Y / _viewModel.CellSize);

                // Mi assicuro che gli indici siano validi
                if (row >= 0 && row < GridSize && col >= 0 && col < GridSize)
                {
                    // Calcolo l'indice piatto per  Cells
                    int index = row * GridSize + col;

                    if (index < _viewModel.Cells.Count)
                    {
                        Cell cell = _viewModel.Cells[index];

                        // Applico il cambio di stato SOLO se è una cella diversa dall'ultima
                        if (cell != _lastClickedCell)
                        {
                            cell.IsActive = _isDrawingMode;
                            _lastClickedCell = cell;
                        }
                    }
                }
            }

            if (WindowState != WindowState.Maximized)
            {
                if (WindowStyle != WindowStyle.SingleBorderWindow)
                {
                    SetFullscreenMode(false);
                }
                return;
            }

            position = e.GetPosition(this);
            bool mouseIsAtTop = position.Y < MouseDetectionArea;

            // Logica ottimizzata: chiama SetFullscreenMode solo se lo stato DEVE cambiare
            if (mouseIsAtTop && WindowStyle != WindowStyle.SingleBorderWindow)
            {
                SetFullscreenMode(false);
            }
            else if (!mouseIsAtTop && WindowStyle != WindowStyle.None)
            {
                SetFullscreenMode(true);
            }
        }

        // Imposto la modalità FullScreen
        private void SetFullscreenMode(bool isFullscreen)
        {
            if (isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResizeWithGrip;
            }

            // Ricalcolo il layout dopo che lo stile della finestra è cambiato.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateHeaderAndGameAreaSize();
            }), DispatcherPriority.Loaded);
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GameArea.IsMouseCaptured)
            {
                GameArea.ReleaseMouseCapture();
            }

            if (Mouse.Captured != null)
            {
                Mouse.Capture(null);
            }

            _lastClickedCell = null;
            e.Handled = true;
        }
    }
}
