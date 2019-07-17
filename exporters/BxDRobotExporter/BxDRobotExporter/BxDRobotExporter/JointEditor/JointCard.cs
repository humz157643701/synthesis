using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EditorsLibrary;

namespace BxDRobotExporter.JointEditor
{
    public sealed partial class JointCard : UserControl
    {
        private readonly JointForm jointForm;
        private readonly RigidNode_Base node;

        private bool isHighlighted;
        private bool hasLoadedEditor;

        public JointCard(RigidNode_Base node, JointForm jointForm)
        {
            this.jointForm = jointForm;
            this.node = node;

            InitializeComponent();

            jointEditor.Initialize(new List<RigidNode_Base> {node}, this);

            AddHighlightAction(this);
        }

        public void LoadValues()
        {
            var joint = node.GetSkeletalJoint();
            jointName.Text = ToStringUtils.NodeNameString(node);
            jointTypeValue.Text = ToStringUtils.JointTypeString(joint);
            driverValue.Text = ToStringUtils.DriverString(joint);
            wheelTypeValue.Text = ToStringUtils.WheelTypeString(joint);
        }

        public void LoadValuesRecursive()
        {
            LoadValues();
            hasLoadedEditor = false;
        }

        private void HighlightSelf()
        {
            if (isHighlighted)
            {
                return;
            }

            jointForm.ResetAllHighlight();
            isHighlighted = true;

            InventorUtils.FocusAndHighlightNode(node, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 0.5);
        }

        public void LoadPreviewIcon()
        {
            var iconCamera = InventorManager.Instance.TransientObjects.CreateCamera();
            iconCamera.SceneObject = StandardAddInServer.Instance.AsmDocument.ComponentDefinition;

            const double zoom = 0.6; // Zoom, where a zoom of 1 makes the camera the size of the whole robot
            
            const int widthConst = 3; // The image needs to be wide to hide the XYZ coordinate labels in the bottom left corner

            var occurrences = InventorUtils.GetComponentOccurrencesFromNodes(new List<RigidNode_Base> {node});
            iconCamera.Fit();
            iconCamera.GetExtents(out _, out var height);

            InventorUtils.SetCameraView(InventorUtils.GetOccurrencesCenter(occurrences), 15, height * zoom * widthConst, height * zoom, iconCamera);


            pictureBox1.Image = AxHostConverter.PictureDispToImage(
                iconCamera.CreateImage(pictureBox1.Height * widthConst, pictureBox1.Height,
                    StandardAddInServer.Instance.MainApplication.TransientObjects.CreateColor(210, 222, 239),
                    StandardAddInServer.Instance.MainApplication.TransientObjects.CreateColor(175, 189, 209)));
        }

        public void ResetHighlight()
        {
            isHighlighted = false;
        }

        private void AddHighlightAction(Control baseControl)
        {
            baseControl.MouseHover += (sender, args) => HighlightSelf();

            foreach (Control control in baseControl.Controls)
                AddHighlightAction(control);
        }

        private void ToggleCollapsed()
        {
            jointForm.CollapseAllCards(this);
            SetCollapsed(!IsCollapsed());
        }

        public void SetCollapsed(bool collapse)
        {
            jointEditor.Visible = !collapse;
        }


        public bool IsCollapsed()
        {
            return !jointEditor.Visible;
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            ToggleCollapsed();
            if (!hasLoadedEditor)
            {
                hasLoadedEditor = true;
                jointEditor.SuspendLayout();
                jointEditor.LoadValues();
                jointEditor.ResumeLayout();
            }
        }

        private void constraintsButton_Click(object sender, EventArgs e)
        {
            var limitEditor = new EditLimits(node.GetSkeletalJoint());// show the limit editor form
            limitEditor.ShowDialog(ParentForm);
        }

        private void sensorsButton_Click(object sender, EventArgs e)
        {
            var listForm = new SensorListForm(node.GetSkeletalJoint());
            listForm.ShowDialog(ParentForm);
        }
    }
}