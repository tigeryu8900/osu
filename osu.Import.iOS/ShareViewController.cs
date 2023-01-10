using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Foundation;
using Social;

namespace osu.Import.iOS
{
    public partial class ShareViewController : SLComposeServiceViewController
    {
        public static void Main(string[] args)
        {
        }

        private NSUserDefaults nsUserDefaults = NSUserDefaults.StandardUserDefaults;

        protected ShareViewController (IntPtr handle) : base (handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void DidReceiveMemoryWarning ()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            List<Task<NSObject>> tasks = new List<Task<NSObject>>();

            if (ExtensionContext?.InputItems != null)
            {
                foreach (NSExtensionItem nsExtensionItem in ExtensionContext?.InputItems!)
                {
                    if (nsExtensionItem.Attachments != null)
                    {
                        foreach (NSItemProvider attachment in nsExtensionItem.Attachments!)
                        {
                            tasks.Add(attachment.LoadItemAsync("public.file-url", null));
                        }
                    }
                }
            }

            Task.Run(async () =>
            {
                List<string> urls = new List<string>();

                foreach (Task<NSObject> task in tasks)
                {
                    if (await task.ConfigureAwait(false) is NSUrl nsUrl)
                    {
                        urls.Add(nsUrl.ToString());
                    }
                }

                nsUserDefaults.SetString(JsonSerializer.Serialize(urls), "imports");
            });

            // Do any additional setup after loading the view.
        }

        public override bool IsContentValid ()
        {
            // Do validation of contentText and/or NSExtensionContext attachments here
            return true;
        }

        public override void DidSelectPost ()
        {
            // This is called after the user selects Post. Do the upload of contentText and/or NSExtensionContext attachments.

            // Inform the host that we're done, so it un-blocks its UI. Note: Alternatively you could call super's -didSelectPost, which will similarly complete the extension context.
            ExtensionContext?.CompleteRequest (Array.Empty<NSExtensionItem>(), null);
        }

        public override SLComposeSheetConfigurationItem[] GetConfigurationItems ()
        {
            // To add configuration options via table cells at the bottom of the sheet, return an array of SLComposeSheetConfigurationItem here.
            return Array.Empty<SLComposeSheetConfigurationItem>();
        }
    }
}
