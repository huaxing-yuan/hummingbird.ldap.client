using Hummingbird.TestFramework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hummingbird.TestFramework.Services
{

    /// <summary>
    /// Interaction logic for ViewLdapEditor.xaml
    /// </summary>
    public partial class ViewLdapEditor : TestFramework.Extensibility.ObjectEditorBase
    {
        DateTimeOffset nextCheckGrammarDate = new DateTimeOffset(DateTime.Now);

        public override string ObjectStringValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Type[] SupportedObjectTypes => new Type[] {typeof(LdapObject)};

        public ViewLdapEditor(Automation.AbstractTestItem testitem) :base(testitem)
        {
            InitializeComponent();
        }

        public ViewLdapEditor(): this(null)
        {
        }

        public override void Sync()
        {
            if (ObjectValue != null)
            {
                txtQuery.Text = ObjectValue.ToString();
            }
            else
            {
                txtQuery.Text = string.Empty;
            }
        }

        private void txtQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            ObjectValue = txtQuery.Text;
        }
    }
}
