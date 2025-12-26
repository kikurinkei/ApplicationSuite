using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Configuration
{
    public class ElementDetail
    {
        public string ElementId { get; set; } = string.Empty;
        public string ElementName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Usage { get; set; }
        public string? Version { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? ElementType { get; set; }
        public string? Category { get; set; }
        public string? Author { get; set; }
        public string? IconPath { get; set; }
        public string? ColorTheme { get; set; }
        public int? TypeFlags { get; set; }  // null許容
        public string? ManualRootDir { get; set; }  // null許容
        public string? ViewModelPath { get; set; }  // null許容
        public string? ControlPath { get; set; }    // null許容（上位層では未使用）
        public Position? InitialPosition { get; set; }
        public Size? InitialSize { get; set; }
        public List<string> ChildElementIds { get; set; } = new();
    }
    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
    public class Size
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }
}