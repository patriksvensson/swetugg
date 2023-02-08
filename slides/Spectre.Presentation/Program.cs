using System;
using Spectre.Console;
using Spectre.Presentation.Framework;

namespace Spectre.Presentation;

public static class Program
{
    public static void Main(string[] args)
    {
        var slide = 0;
        if (args.Length == 1)
        {
            slide = int.Parse(args[0]) - 1;
        }

        Slideshow.Run(new Slide[]
        {
            new CoverSlide(),
            new AboutSlide(),
            new RickRollSlide(showHeader: false),
            new WordlistSlide(),
            new WhatIsSpectreConsole(),
            new WhatIsNotSpectreConsole(),
            new WhatProblemsDoesItSolve(),
            new ColorsSlide(),
            new TextSlide(),
            new TableSlide(),
            new TableDemoSlide(),
            new TreeSlide(),
            new TreeDemoSlide(),
            //new ChartsSlide(),
            new WhatElseSlide(),
            new LayoutSlide(),
            new DemoSlide(),
            new ThanksSlide(),
        }, Math.Max(0, slide));
    }
}
