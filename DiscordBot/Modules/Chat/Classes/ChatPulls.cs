using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Modules.Chat.Classes
{
    class ChatPulls
    {
        public string status { get; set; }
        public string message { get; set; }
    }

    class Kitty
    {

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.Serializable]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public class response
        {

            private responseData dataField;

            /// <remarks/>
            public responseData data
            {
                get
                {
                    return this.dataField;
                }
                set
                {
                    this.dataField = value;
                }
            }
        }

        /// <remarks/>
        [System.Serializable]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class responseData
        {

            private responseDataImages imagesField;

            /// <remarks/>
            public responseDataImages images
            {
                get
                {
                    return this.imagesField;
                }
                set
                {
                    this.imagesField = value;
                }
            }
        }

        /// <remarks/>
        [System.Serializable]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class responseDataImages
        {

            private responseDataImagesImage imageField;

            /// <remarks/>
            public responseDataImagesImage image
            {
                get
                {
                    return this.imageField;
                }
                set
                {
                    this.imageField = value;
                }
            }
        }

        /// <remarks/>
        [System.Serializable]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class responseDataImagesImage
        {

            private string urlField;

            private float idField;

            private string source_urlField;

            /// <remarks/>
            public string url
            {
                get
                {
                    return this.urlField;
                }
                set
                {
                    this.urlField = value;
                }
            }

            /// <remarks/>
            public float id
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }

            /// <remarks/>
            public string source_url
            {
                get
                {
                    return this.source_urlField;
                }
                set
                {
                    this.source_urlField = value;
                }
            }
        }


    }

}
