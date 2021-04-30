using AntDesign;
using System.Application.Models;

namespace System.Application.Pages.Form
{
    public class FormItemLayout
    {
        public ColLayoutParam LabelCol { get; set; }
        public ColLayoutParam WrapperCol { get; set; }
    }

    public partial class BasicForm
    {
        private readonly BasicFormModel _model = new();

        private readonly FormItemLayout _formItemLayout = new()
        {
            LabelCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty {Span = 24},
                Sm = new EmbeddedProperty {Span = 7},
            },

            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty {Span = 24},
                Sm = new EmbeddedProperty {Span = 12},
                Md = new EmbeddedProperty {Span = 10},
            }
        };

        private readonly FormItemLayout _submitFormLayout = new()
        {
            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty { Span = 24, Offset = 0},
                Sm = new EmbeddedProperty { Span = 10, Offset = 7},
            }
        };

        private void HandleSubmit()
        {
        }
    }
}