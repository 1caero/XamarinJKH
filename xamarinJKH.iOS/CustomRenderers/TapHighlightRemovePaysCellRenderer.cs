using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using xamarinJKH.Main;
using xamarinJKH.iOS.CustomRenderers;
using xamarinJKH.Pays;

[assembly: ExportRenderer(typeof(HistoryPaysCell), typeof(TapHighlightRemovePaysCellRenderer))]
[assembly: ExportRenderer(typeof(PaysCell), typeof(TapHighlightRemovePaysCellRenderer))]
namespace xamarinJKH.iOS.CustomRenderers
{
    public class TapHighlightRemovePaysCellRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);
            // убираем стиль по-умлочанию - серый цвет вокруг поля ячейки при нажатии
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            return cell;
        }

    }
}
