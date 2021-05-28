using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace UCalc.Controls
{
    public class PrintableTextBox : PrintableDocument<TextBox>
    {
        public PrintableTextBox(TextBox textBox) : base(textBox)
        {
        }

        protected override void FillFlowDocument(FlowDocument document, TextBox textBox)
        {
            var section = new Section {BreakPageBefore = true, FontSize = Constants.PrintDefaultFontSize};
            document.Blocks.Add(section);

            var table = new Table {CellSpacing = 0};
            section.Blocks.Add(table);
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            AddText(rowGroup, textBox.Text);
        }
    }
}