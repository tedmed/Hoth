using System;
using System.Collections.Generic;
using System.Text;

namespace ChmiCapAlertProvider.XmlModels
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EXPORT
    {

        private EXPORTINFO iNFOField;

        private EXPORTPOLOZKAPOLVAZ[][] dATAField;

        /// <remarks/>
        public EXPORTINFO INFO
        {
            get
            {
                return this.iNFOField;
            }
            set
            {
                this.iNFOField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("POLOZKA", IsNullable = false)]
        [System.Xml.Serialization.XmlArrayItemAttribute("POLVAZ", IsNullable = false, NestingLevel = 1)]
        public EXPORTPOLOZKAPOLVAZ[][] DATA
        {
            get
            {
                return this.dATAField;
            }
            set
            {
                this.dATAField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EXPORTINFO
    {

        private EXPORTINFOTYPEXP tYPEXPField;

        private decimal vERZEEXPField;

        private string zDROJEXPField;

        private System.DateTime dATEXPField;

        private System.DateTime dATPOHLField;

        private string kODJAZField;

        private EXPORTINFOVAZBA[] oBSAHField;

        /// <remarks/>
        public EXPORTINFOTYPEXP TYPEXP
        {
            get
            {
                return this.tYPEXPField;
            }
            set
            {
                this.tYPEXPField = value;
            }
        }

        /// <remarks/>
        public decimal VERZEEXP
        {
            get
            {
                return this.vERZEEXPField;
            }
            set
            {
                this.vERZEEXPField = value;
            }
        }

        /// <remarks/>
        public string ZDROJEXP
        {
            get
            {
                return this.zDROJEXPField;
            }
            set
            {
                this.zDROJEXPField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime DATEXP
        {
            get
            {
                return this.dATEXPField;
            }
            set
            {
                this.dATEXPField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime DATPOHL
        {
            get
            {
                return this.dATPOHLField;
            }
            set
            {
                this.dATPOHLField = value;
            }
        }

        /// <remarks/>
        public string KODJAZ
        {
            get
            {
                return this.kODJAZField;
            }
            set
            {
                this.kODJAZField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("VAZBA", IsNullable = false)]
        public EXPORTINFOVAZBA[] OBSAH
        {
            get
            {
                return this.oBSAHField;
            }
            set
            {
                this.oBSAHField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EXPORTINFOTYPEXP
    {

        private byte kodField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte kod
        {
            get
            {
                return this.kodField;
            }
            set
            {
                this.kodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EXPORTINFOVAZBA
    {

        private EXPORTINFOVAZBATYPCIS tYPCISField;

        private string aKRCISField;

        private uint kODCISField;

        private string nAZEVField;

        private byte refField;

        /// <remarks/>
        public EXPORTINFOVAZBATYPCIS TYPCIS
        {
            get
            {
                return this.tYPCISField;
            }
            set
            {
                this.tYPCISField = value;
            }
        }

        /// <remarks/>
        public string AKRCIS
        {
            get
            {
                return this.aKRCISField;
            }
            set
            {
                this.aKRCISField = value;
            }
        }

        /// <remarks/>
        public uint KODCIS
        {
            get
            {
                return this.kODCISField;
            }
            set
            {
                this.kODCISField = value;
            }
        }

        /// <remarks/>
        public string NAZEV
        {
            get
            {
                return this.nAZEVField;
            }
            set
            {
                this.nAZEVField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte @ref
        {
            get
            {
                return this.refField;
            }
            set
            {
                this.refField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EXPORTINFOVAZBATYPCIS
    {

        private string kodField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string kod
        {
            get
            {
                return this.kodField;
            }
            set
            {
                this.kodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EXPORTPOLOZKAPOLVAZ
    {

        private string cHODNOTAField;

        private string tEXTField;

        private byte refField;

        /// <remarks/>
        public string CHODNOTA
        {
            get
            {
                return this.cHODNOTAField;
            }
            set
            {
                this.cHODNOTAField = value;
            }
        }

        /// <remarks/>
        public string TEXT
        {
            get
            {
                return this.tEXTField;
            }
            set
            {
                this.tEXTField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte @ref
        {
            get
            {
                return this.refField;
            }
            set
            {
                this.refField = value;
            }
        }
    }


}
