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

            builder.AddCustomAttributes(typeof(ParseDocumentWithHumanInTheLoop), categoryAttribute);
            builder.AddCustomAttributes(typeof(ParseDocumentWithHumanInTheLoop), new DesignerAttribute(typeof(ParseDocumentWithHumanInTheLoopDesigner)));
            builder.AddCustomAttributes(typeof(ParseDocumentWithHumanInTheLoop), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(ListProcessedDocuments), categoryAttribute);
            builder.AddCustomAttributes(typeof(ListProcessedDocuments), new DesignerAttribute(typeof(ListProcessedDocumentsDesigner)));
            builder.AddCustomAttributes(typeof(ListProcessedDocuments), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(MarkRunAsCompleted), categoryAttribute);
            builder.AddCustomAttributes(typeof(MarkRunAsCompleted), new DesignerAttribute(typeof(MarkRunAsCompletedDesigner)));
            builder.AddCustomAttributes(typeof(MarkRunAsCompleted), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(ParseDocument), categoryAttribute);
            builder.AddCustomAttributes(typeof(ParseDocument), new DesignerAttribute(typeof(ParseDocumentDesigner)));
            builder.AddCustomAttributes(typeof(ParseDocument), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
