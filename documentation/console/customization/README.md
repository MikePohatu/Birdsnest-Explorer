# Customization

* [Customization](#customization)
	* [Overview](#overview)
	* [Included frameworks and styling](#included-frameworks-and-styling)
		* [Example](#example)
		* [Global Styles](#global-styles)

---


## Overview

The Birdsnest Explorer [console](/documentation/console/README.md) has some customization options to include custom branding and messaging for your environment. These can be included on the login and portal pages. 

The following files can be edited to insert content into the console. These files can be found in the **wwwroot\static\customization** console sub-folder:

* **custom.css** - custom styling can be defined here
* **login_banner.htm** - HTML to be inserted above the login dialog on the Login page
* **login_footer.htm** - HTML to be inserted below the login dialog on the Login page
* **portal_banner.htm** - HTML to be inserted above the standard content on the portal page
* **portal_footer.htm** - HTML to be inserted below the standard content on the portal page

Additional files can be copied  into the _wwwroot\static\customization_ folder so they can be referenced by the files above. Files should not be copied to other subfolders of the wwwroot or static folders as they may not be copied during upgrades by the managed installer.

HTML may be entered into the files above and will be copied directly into the console within a \<div> in the relevant location. **custom.css** is defined in the head of the page and is applied last to supersede the default css. Note that due to scoped css applied in the console, you may need to apply the [!important](https://www.w3schools.com/css/css_important.asp) css tag in some cases.

## Included frameworks and styling

Biredsnest Explorer makes use of [Foundation 6](https://get.foundation/sites/docs/) and [FontAwesome](https://fontawesome.com/icons?d=gallery&m=free) (the free version). You can use the classes and unicode characters from these projects to style your custom content. The example makes use of [XY Grid](https://get.foundation/sites/docs/xy-grid.html) from Foundation to center an image/logo: 


### Example

```html
<div class="grid-x">
    <div class="cell auto"></div>
    <div class="cell shrink">
        <img src="/static/customization/img/Logo_128x128.png">
    </div>
    <div class="cell auto"></div>
</div>
```

### Global Styles

The following css is applied globally so it can be referenced throughout the console:


```css
.fillAvailable {
	margin: 0;
	padding: 0;
	height: 100%;
	width: 100%;
}

.page {
	padding-top: 10px;
	padding-bottom: 10px;
	padding-right: 20px;
	padding-left: 20px;
}

.clickable:hover {
	cursor: pointer;
}

.has-tip {
	border-bottom: none;
	cursor: auto;
}

.hidden {
	display: none;
}

.xy-center {
	position: absolute;
	top: 50%;
	left: 50%;
	transform: translate(-50%, -50%);
}

.x-center {
	position: absolute;
	left: 50%;
	transform: translate(-50%, 0);
}

.absolute-top-left {
	position: absolute;
	top: 0;
	left: 0;
}

.scrollable {
	overflow: auto;
}

.scrollable::-webkit-scrollbar {
	width: 10px;
	background: transparent;
}

.scrollable::-webkit-scrollbar-thumb {
	background: #d6d6d6;
}

.spinner {
	-webkit-animation: spin 4s linear infinite;
	-moz-animation: spin 4s linear infinite;
	animation: spin 4s linear infinite;
}

@-moz-keyframes spin {
	100% {
		-moz-transform: rotate(360deg);
	}
}

@-webkit-keyframes spin {
	100% {
		-webkit-transform: rotate(360deg);
	}
}

@keyframes spin {
	100% {
		-webkit-transform: rotate(360deg);
		transform: rotate(360deg);
		-ms-transform-origin: 50%;
	}
}

.dialog {
	padding: 5px 15px;
	background: rgba(255, 255, 255, 1);
	opacity: 1;
}

.dialogWrapper {
	padding: 5px;
	background-color: rgba(220, 220, 220, 0.8);
	z-index: 4000;
	position: fixed;
	top: 50%;
	left: 50%;
	transform: translate(-50%, -50%);
}

@media all and (max-width: 767px) {
	.dialogWrapper {
		position: fixed;
		top: 50%;
		left: 0;
		right: 0;
		transform: translate(0, -50%);
	}
}

.icon {
	font-family: "Font Awesome 5 Free";
	font-weight: 900;
	text-anchor: middle;
	stroke-width: 0;
	line-height: 1;
}

.noselect {
	-webkit-touch-callout: none;
	-webkit-user-select: none;
	-khtml-user-select: none;
	-moz-user-select: none;
	-ms-user-select: none;
	user-select: none;
}

.loading::after {
	content: ".";
	animation: dots 4s steps(5, end) infinite;
}

@keyframes dots {
	0%,
	20% {
		color: rgba(0, 0, 0, 0);
		text-shadow: 0.2em 0 0 rgba(0, 0, 0, 0), 0.4em 0 0 rgba(0, 0, 0, 0);
	}
	40% {
		color: #606060;
		text-shadow: 0.2em 0 0 rgba(0, 0, 0, 0), 0.4em 0 0 rgba(0, 0, 0, 0);
	}
	60% {
		text-shadow: 0.2em 0 0 #606060, 0.4em 0 0 rgba(0, 0, 0, 0);
	}
	80%,
	100% {
		text-shadow: 0.2em 0 0 #606060, 0.4em 0 0 #606060;
	}
}
```