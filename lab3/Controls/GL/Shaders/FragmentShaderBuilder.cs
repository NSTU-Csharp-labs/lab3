using System.Collections.Generic;
using Avalonia.Animation;

namespace lab3.Controls.GL.Shaders;

public class FragmentShaderBuilder
{
    private bool IsWBActivated;
    
    public FragmentShaderBuilder(IEnumerable<Filters> activatedFilters)
    {
        foreach (var filter in activatedFilters)
        {
            var f = filter switch
                {
                    Filters.BlackAndWhite => BlackAndWhite,
                    Filters.Red => Red,
                    Filters.Blue => Blue,
                    Filters.Green => Green
                };
            AddedDeclarations += f.Source + "\n";
            AddedFuncs += $"color = {f.Name}(color);\n";
        }
    }
    
    
    public string createFragmentShader()
    {
        return FragmentDeclarations + 
               AddedDeclarations +
               FragmentBeforeApplyingFilters +
               AddedFuncs +
               FragmentAfterApplyingFilters;
    }

    private string AddedDeclarations;
    private string AddedFuncs;
    
    private const string FragmentDeclarations = @"
    varying vec2 vTexCoord;
    uniform sampler2D uTexture;
    ";

    private const string FragmentBeforeApplyingFilters = @"
    void main()
    {
        vec4 color = texture(uTexture, vTexCoord);
    ";       

    private const string FragmentAfterApplyingFilters = @"
        gl_FragColor = color;
    }
    ";

    private Filter BlackAndWhite = new Filter(
        @"
            vec4 blackAndWhite(vec4 color)
            {
                float grey = 0.21 * color.r + 0.71 * color.g + 0.07 * color.b;
                return vec4(grey , grey, grey, 1.0);
            }
              ",
        @"blackAndWhite"
        );
    
    private Filter Red = new Filter(
        @"",
        @""
    );
    
    private Filter Blue = new Filter(
        @"",
        @""
    );
    
    private Filter Green = new Filter(
        "",
        ""
    );

}