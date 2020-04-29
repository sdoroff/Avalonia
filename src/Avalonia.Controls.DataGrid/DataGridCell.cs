// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents an individual <see cref="T:Avalonia.Controls.DataGrid" /> cell.
    /// </summary>
    public class DataGridCell : ContentControl
    {
        private const string DATAGRIDCELL_elementRightGridLine = "PART_RightGridLine";

        private Rectangle _rightGridLine;
        private DataGridColumn _owningColumn;

        bool _isValid;

        public static readonly DirectProperty<DataGridCell, bool> IsValidProperty =
            AvaloniaProperty.RegisterDirect<DataGridCell, bool>(
                nameof(IsValid),
                o => o.IsValid);

        public void AddToLog(RowLogData data)
        {
            if (IsTesting)
                OwningRow.AddToLog(data);
        }

        private void Log(string type, string message)
        {
            var data =
                new RowLogData()
                {
                    Source = "Cell",
                    Type = type,
                    Message = message
                };
            AddToLog(data);
        }
        private void LogLayout(string message)
        {
            Log("Layout", message);
        }
        private void LogProperty(string property, string message)
        {
            var data =
                new RowLogData()
                {
                    Source = "Cell",
                    Type = "Property",
                    Property = property,
                    Message = message
                };

            AddToLog(data);
        }

        private void LogContent(string type, string message)
        {
            var data =
                new RowLogData()
                {
                    Source = "Content",
                    Type = type,
                    Message = message
                };
            AddToLog(data);
        }
        private void LogContentLayout(string message)
        {
            LogContent("Layout", message);
        }
        private void LogContentProperty(string property, string message)
        {
            var data =
                new RowLogData()
                {
                    Source = "Content",
                    Type = "Property",
                    Property = property,
                    Message = message
                };

            AddToLog(data);
        }

        static DataGridCell()
        {
            PointerPressedEvent.AddClassHandler<DataGridCell>(
                (x,e) => x.DataGridCell_PointerPressed(e), handledEventsToo: true);
        }
        public DataGridCell()
        {
            this.LayoutUpdated += (s, e) => LogLayout("LayoutUpdated");
        }

        protected override void OnDataContextBeginUpdate()
        {
            Log("DataContext", "DataContext Update Start");
            base.OnDataContextBeginUpdate();
        }
        protected override void OnDataContextEndUpdate()
        {
            base.OnDataContextEndUpdate();
            Log("DataContext", "DataContext Update Complete");
        }
        protected override void OnPropertyChanged<T>(AvaloniaProperty<T> property, Optional<T> oldValue, BindingValue<T> newValue, BindingPriority priority)
        {
            if (newValue.HasValue && newValue.Value != null)
            {
                var oldValueText = "NIL";
                if (oldValue.HasValue && oldValue.Value != null)
                    oldValueText = oldValue.Value.ToString();
                LogProperty(property.Name, $"{property.Name} Changed ({oldValueText} -> {newValue.Value})");
            }

            if(property == ContentProperty && newValue.HasValue && newValue.Value is TextBlock element)
            {
                LogLayout("Content Set and Bound");
                element.LayoutUpdated += (s, e) => LogContentLayout("Layout Updated");
                element.PropertyChanged += Element_PropertyChanged;
            }

            base.OnPropertyChanged(property, oldValue, newValue, priority);
        }

        private void Element_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            LogContentProperty(e.Property.Name, $"{e.Property.Name} Changed ({e.OldValue} -> {e.NewValue})");
        } 

        protected override Size MeasureOverride(Size availableSize)
        {
            //if(IsTargetCell)
            LogLayout($"Measure Start ({availableSize.Height})");

            var result = base.MeasureOverride(availableSize);
            LogLayout($"Measure Complete ({result.Height})");
            return result;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            LogLayout($"Arrange Start ({finalSize.Height})");
            var result = base.ArrangeOverride(finalSize);
            LogLayout($"Measure Complete ({result.Height})");
            return result;
        }

        public bool IsValid
        {
            get { return _isValid; }
            internal set { SetAndRaise(IsValidProperty, ref _isValid, value); }
        }

        internal DataGridColumn OwningColumn
        {
            get => _owningColumn;
            set
            {
                if (_owningColumn != value)
                {
                    _owningColumn = value;
                    OnOwningColumnSet(value);
                }
            }
        }
        internal DataGridRow OwningRow
        {
            get;
            set;
        }

        internal DataGrid OwningGrid
        {
            get { return OwningRow?.OwningGrid ?? OwningColumn?.OwningGrid; }
        }

        private bool TestColumn
        {
            get { return OwningColumn?.TestColumn ?? false; }
        }

        private bool IsTesting
        {
            get { return (OwningGrid?.IsTestElement ?? false) && TestColumn; }
        }

        private bool IsTargetCell
        {
            get { return (OwningRow?.IsTargetRow ?? false) && TestColumn; }
        }

        internal double ActualRightGridLineWidth
        {
            get { return _rightGridLine?.Bounds.Width ?? 0; }
        }

        internal int ColumnIndex
        {
            get { return OwningColumn?.Index ?? -1; }
        }

        internal int RowIndex
        {
            get { return OwningRow?.Index ?? -1; }
        }

        internal bool IsCurrent
        {
            get
            {
                return OwningGrid.CurrentColumnIndex == OwningColumn.Index &&
                       OwningGrid.CurrentSlot == OwningRow.Slot;
            }
        }

        private bool IsEdited
        {
            get
            {
                return OwningGrid.EditingRow == OwningRow &&
                       OwningGrid.EditingColumnIndex == ColumnIndex;
            }
        }

        private bool IsMouseOver
        {
            get
            {
                return OwningRow != null && OwningRow.MouseOverColumnIndex == ColumnIndex;
            }
            set
            {
                if (value != IsMouseOver)
                {
                    if (value)
                    {
                        OwningRow.MouseOverColumnIndex = ColumnIndex;
                    }
                    else
                    {
                        OwningRow.MouseOverColumnIndex = null;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the visual tree for the cell control when a new template is applied.
        /// </summary>
        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            UpdatePseudoClasses();
            _rightGridLine = e.NameScope.Find<Rectangle>(DATAGRIDCELL_elementRightGridLine);
            if (_rightGridLine != null && OwningColumn == null)
            {
                // Turn off the right GridLine for filler cells
                _rightGridLine.IsVisible = false;
            }
            else
            {
                EnsureGridLine(null);
            }

        }
        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);

            if (OwningRow != null)
            {
                IsMouseOver = true;
            }
        }
        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);

            if (OwningRow != null)
            {
                IsMouseOver = false;
            }
        }

        //TODO TabStop
        private void DataGridCell_PointerPressed(PointerPressedEventArgs e)
        {
            // OwningGrid is null for TopLeftHeaderCell and TopRightHeaderCell because they have no OwningRow
            if (OwningGrid != null)
            {
                OwningGrid.OnCellPointerPressed(new DataGridCellPointerPressedEventArgs(this, OwningRow, OwningColumn, e));
                if (e.MouseButton == MouseButton.Left)
                {
                    if (!e.Handled)
                    //if (!e.Handled && OwningGrid.IsTabStop)
                    {
                        OwningGrid.Focus();
                    }
                    if (OwningRow != null)
                    {
                        e.Handled = OwningGrid.UpdateStateOnMouseLeftButtonDown(e, ColumnIndex, OwningRow.Slot, !e.Handled);
                        OwningGrid.UpdatedStateOnMouseLeftButtonDown = true;
                    }
                }
            }
        }

        internal void UpdatePseudoClasses()
        {

        }

        // Makes sure the right gridline has the proper stroke and visibility. If lastVisibleColumn is specified, the 
        // right gridline will be collapsed if this cell belongs to the lastVisibileColumn and there is no filler column
        internal void EnsureGridLine(DataGridColumn lastVisibleColumn)
        {
            if (OwningGrid != null && _rightGridLine != null)
            {
                if (OwningGrid.VerticalGridLinesBrush != null && OwningGrid.VerticalGridLinesBrush != _rightGridLine.Fill)
                {
                    _rightGridLine.Fill = OwningGrid.VerticalGridLinesBrush;
                }

                bool newVisibility =
                    (OwningGrid.GridLinesVisibility == DataGridGridLinesVisibility.Vertical || OwningGrid.GridLinesVisibility == DataGridGridLinesVisibility.All)
                        && (OwningGrid.ColumnsInternal.FillerColumn.IsActive || OwningColumn != lastVisibleColumn);

                if (newVisibility != _rightGridLine.IsVisible)
                {
                    _rightGridLine.IsVisible = newVisibility;
                }
            }
        }

        private void OnOwningColumnSet(DataGridColumn column)
        {
            if (column == null)
            {
                Classes.Clear();
            }
            else
            {
                Classes.Replace(column.CellStyleClasses);
            }
        }
    }
}
