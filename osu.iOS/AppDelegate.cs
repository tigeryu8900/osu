// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Foundation;
using osu.Framework.iOS;
using UIKit;

namespace osu.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : GameAppDelegate
    {
        private NSUserDefaults nsUserDefaults = NSUserDefaults.StandardUserDefaults;

        private OsuGameIOS game;

        public AppDelegate()
        {
            nsUserDefaults.AddObserver("imports", NSKeyValueObservingOptions.New, change =>
            {
                List<String> urls = JsonSerializer.Deserialize<List<String>>((NSString)change.NewValue);

                foreach (String urlStr in urls)
                {
                    NSUrl url = new NSUrl(urlStr);
                    if (url.IsFileUrl)
                        Task.Run(() => game?.Import(url.Path));
                    else
                        Task.Run(() => game?.HandleLink(url.AbsoluteString));
                }
            });
        }

        protected override Framework.Game CreateGame() => game = new OsuGameIOS();

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (url.IsFileUrl)
                Task.Run(() => game.Import(url.Path));
            else
                Task.Run(() => game.HandleLink(url.AbsoluteString));
            return true;
        }
    }
}
