using System.Collections.Generic;
using System.Windows.Forms;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Editors.JointEditor
{
    public partial class JointFormSimple : Form
    {
        private readonly List<MiniJointCard> jointCards = new List<MiniJointCard>();

        public JointFormSimple()
        {
            AnalyticsUtils.LogPage("Joint Editor");
            InitializeComponent();

            Shown += (sender, args) => // First load
            {
                jointCards.ForEach(card => card.LoadPreviewIcon());
            };

            Closing += (sender, e) => // Every close
            {
                InventorUtils.FocusAndHighlightNodes(null, RobotExporterAddInServer.Instance.Application.ActiveView.Camera, 1);
            };

            FormClosing += (sender, e) =>
            {
                Hide();
                e.Cancel = true; // this cancels the close event.
            };
        }

        public void UpdateSkeleton(RobotDataManager robotDataManager)
        {
            SuspendLayout();
            
            DefinePartsLayout.Controls.Clear();
            DefinePartsLayout.RowStyles.Clear();

            foreach (RigidNode_Base node in robotDataManager.RobotBaseNode.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null) // create new part panels for every node
                {
                    var panel = new MiniJointCard(node, this, robotDataManager) {Dock = DockStyle.Top};

                    jointCards.Add(panel);

                    AddControlToNewTableRow(panel, DefinePartsLayout);
                }
            }

            ResumeLayout();
        }
        
        public static void AddControlToNewTableRow(Control control, TableLayoutPanel table, RowStyle rowStyle = null)
        {
            if (rowStyle == null)
            {
                rowStyle = new RowStyle();
                rowStyle.SizeType = SizeType.AutoSize;
            }

            table.RowCount++;
            table.RowStyles.Add(rowStyle);
            table.Controls.Add(control);
            table.SetRow(control, table.RowCount - 2);
            table.SetColumn(control, 0);
        }

        public new void ShowDialog()
        {
            jointCards.ForEach(card => card.LoadValues());
            base.ShowDialog();
        }

        public void ResetAllHighlight()
        {
            jointCards.ForEach(card => card.ResetHighlight());
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            jointCards.ForEach(card => card.SaveValues());
            Close();
        }
    }
}