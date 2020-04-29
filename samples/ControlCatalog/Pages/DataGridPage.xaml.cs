using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ControlCatalog.Models;
using Avalonia.Collections;
using System;
using Avalonia.LogicalTree;
using System.Linq;
using System.Diagnostics;
using Avalonia.VisualTree;

namespace ControlCatalog.Pages
{
    public class DataGridPage : UserControl
    {
        public DataGridPage()
        {
            this.InitializeComponent();
            var dg1 = this.FindControl<DataGrid>("dataGrid1");
            dg1.IsReadOnly = true;

            var collectionView1 = new DataGridCollectionView(Countries.All);
            //collectionView.GroupDescriptions.Add(new PathGroupDescription("Region"));

            dg1.Items = collectionView1;

            var dg2 = this.FindControl<DataGrid>("dataGridGrouping");
            dg2.IsReadOnly = true;

            var collectionView2 = new DataGridCollectionView(Countries.All);
            collectionView2.GroupDescriptions.Add(new DataGridPathGroupDescription("Region"));

            dg2.Items = collectionView2;

            var dg3 = this.FindControl<DataGrid>("dataGridEdit");
            dg3.IsReadOnly = false;

            var items = new List<Person>
            {
                new Person { FirstName = "John", LastName = "Doe" },
                new Person { FirstName = "Elizabeth", LastName = "Thomas" },
                new Person { FirstName = "Zack", LastName = "Ward" }
            };
            var collectionView3 = new DataGridCollectionView(items);

            dg3.Items = collectionView3;

            var dg4 = this.FindControl<DataGrid>("dataGridRowHeights");
            dg4.IsTestElement = true;
            dg4.IsReadOnly = false;

            var rowHeights = new List<Person>();
            var rowIndex = 0;
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 4; j++)
                    rowHeights.Add(new Person { FirstName = "a", LastName = (rowIndex++).ToString() });
                rowHeights.Add(new Person { FirstName = "b\nb\nb", LastName = (rowIndex++).ToString() });
            }

            var collectionView4 = new DataGridCollectionView(rowHeights);

            dg4.Items = collectionView4;

            var btnFind = this.FindControl<Button>("btnFind");
            var tbInput = this.FindControl<TextBox>("tbInput");

            string getInputText()
            {
                if (String.IsNullOrEmpty(tbInput.Text))
                    return null;
                else
                    return tbInput.Text.Trim();
            }
            void showRowLog(DataGridRow row)
            {
                row.PrintLog();
            }
            IEnumerable<DataGridRow> getAllRows()
            {
                return
                    dg4.GetVisualDescendants()
                       .OfType<DataGridRow>();
            }
            void processShowLog()
            {
                var inputText = getInputText();
                if(!String.IsNullOrEmpty(inputText))
                {
                    var row =
                        getAllRows().FirstOrDefault(row =>
                            {
                                if (row.DataContext is Person person)
                                    return person.LastName == inputText;
                                else
                                    return false;
                            });

                    if(row != null)
                        showRowLog(row);
                }
            }
            btnFind.Click += (a, b) => processShowLog();
    
            var addButton = this.FindControl<Button>("btnAdd");
            addButton.Click += (a, b) => collectionView3.AddNew();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
