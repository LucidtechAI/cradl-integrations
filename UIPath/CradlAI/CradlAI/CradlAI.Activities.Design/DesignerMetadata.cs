using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using CradlAI.Activities.Design.Designers;
using CradlAI.Activities.Design.Properties;

namespace CradlAI.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(ListFlows), categoryAttribute);
            builder.AddCustomAttributes(typeof(ListFlows), new DesignerAttribute(typeof(ListFlowsDesigner)));
            builder.AddCustomAttributes(typeof(ListFlows), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(ParseDocument), categoryAttribute);
            builder.AddCustomAttributes(typeof(ParseDocument), new DesignerAttribute(typeof(ParseDocumentDesigner)));
            builder.AddCustomAttributes(typeof(ParseDocument), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
