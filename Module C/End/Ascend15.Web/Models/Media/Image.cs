using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;

namespace Ascend15.Models.Media
{
    [ContentType(DisplayName = "Image File", GUID = "8092A593-7A8E-4DE8-A2B3-8C605B70728E", Description = "")]
    [MediaDescriptor(ExtensionString = "jpg,jpeg,gif,png")]
    public class Image : ImageData
    {
        public virtual string Description { get; set; }
    }
}
