using Microsoft.AspNet.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Host.UI.TagHelpers
{
    [HtmlTargetElement(Attributes = "hide-when")]
    public class HideWhenTagHelper : TagHelper
    {
        [HtmlAttributeName("hide-when")]
        public bool HideWhen { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (HideWhen)
            {
                output.SuppressOutput();
            }
        }
    }

    [HtmlTargetElement(Attributes = "show-when")]
    public class ShowWhenTagHelper : TagHelper
    {
        [HtmlAttributeName("show-when")]
        public bool ShowWhen { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ShowWhen == false)
            {
                output.SuppressOutput();
            }
        }
    }

    [HtmlTargetElement(Attributes = "hide-when-null")]
    public class HideWhenNullTagHelper : TagHelper
    {
        [HtmlAttributeName("hide-when-null")]
        public object HideWhenNull { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (HideWhenNull == null)
            {
                output.SuppressOutput();
            }
        }
    }

    [HtmlTargetElement(Attributes = "show-when-null")]
    public class ShowWhenNullTagHelper : TagHelper
    {
        [HtmlAttributeName("show-when-null")]
        public object ShowWhenNull { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ShowWhenNull != null)
            {
                output.SuppressOutput();
            }
        }
    }
}
