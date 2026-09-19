# Blog visuals

`abp-subscrio-cover.jpg` is the generated article header and cover image for the ABP Community listing, compressed below 1 MB. It appears beneath the article title, separate from the architecture diagram in the introduction.

The [article](../blog.md) embeds `architecture-flow.png` after the introduction and `subscrio-server-dashboard.png` in the Server section. Both images are stored in this directory and use absolute raw GitHub URLs so they can load when the article is imported outside GitHub. The dashboard PNG is approximately 244 KB.

`architecture-flow.png` is a PNG export of the original `architecture-flow.svg`. The other diagrams below are supporting repository assets, not embedded in the article. If you add one to the ABP article, export it as PNG or JPG to follow [ABP's media guidelines](https://abp.io/blog-guidelines).

Each diagram is available as an SVG for direct use on the web and as Mermaid source for editing.

| Diagram | SVG | Editable source |
| --- | --- | --- |
| ABP and Subscrio architecture | [`architecture-flow.svg`](architecture-flow.svg) | [`architecture-flow.mmd`](architecture-flow.mmd) |
| Customer entitlement resolution | [`entitlement-resolution.svg`](entitlement-resolution.svg) | [`entitlement-resolution.mmd`](entitlement-resolution.mmd) |
| Stripe event flow | [`stripe-event-flow.svg`](stripe-event-flow.svg) | [`stripe-event-flow.mmd`](stripe-event-flow.mmd) |

The SVGs use a 1200-pixel-wide view box and remain sharp when resized. Text stays as text, so the files can be adjusted in Figma, Illustrator, Inkscape, or a text editor.
