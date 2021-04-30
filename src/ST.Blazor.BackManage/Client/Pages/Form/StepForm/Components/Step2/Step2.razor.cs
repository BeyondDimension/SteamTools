using AntDesign;
using System.Application.Models;
using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.Form
{
    public partial class Step2
    {
        private readonly StepFormModel _model = new();
        private readonly FormItemLayout _formLayout = new()
        {
            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty { Span = 24, Offset = 0 },
                Sm = new EmbeddedProperty { Span = 19, Offset = 5 },
            }
        };

        [CascadingParameter] public StepForm StepForm { get; set; }

        public void OnValidateForm()
        {
            StepForm.Next();
        }

        public void Preview()
        {
            StepForm.Prev();
        }
    }
}