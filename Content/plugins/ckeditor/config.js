/**
 * @license Copyright (c) 2003-2023, CKSource Holding sp. z o.o. All rights reserved.
 * For licensing, see https://ckeditor.com/legal/ckeditor-oss-license
 * 
 * CKEditor 4 Configuration for Persian/RTL Medical Environment
 * تنظیمات CKEditor برای محیط فارسی و راست‌به‌چپ - کلینیک شفا
 */

CKEDITOR.editorConfig = function( config ) {
	// ========================================
	// تنظیمات زبان و جهت - Language & Direction
	// ========================================
	config.language = 'fa'; // زبان فارسی
	config.contentsLangDirection = 'rtl'; // جهت راست به چپ
	config.uiColor = '#f5f5f5'; // رنگ پس‌زمینه رابط کاربری
	
	// ========================================
	// تنظیمات فونت - Font Settings
	// ========================================
	config.font_names = 'Tahoma;Arial;Verdana;Times New Roman;Courier New';
	config.fontSize_defaultLabel = '14px';
	config.fontSize_sizes = '9/9px;10/10px;11/11px;12/12px;13/13px;14/14px;16/16px;18/18px;20/20px;22/22px;24/24px;28/28px;32/32px';
	
	// ========================================
	// تنظیمات Toolbar - Toolbar Configuration
	// ========================================
	// Toolbar بهینه‌شده برای محیط فارسی و درمانی
	config.toolbar = [
		{ name: 'clipboard', items: [ 'Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo' ] },
		{ name: 'editing', items: [ 'Find', 'Replace', '-', 'SelectAll' ] },
		{ name: 'basicstyles', items: [ 'Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat' ] },
		{ name: 'paragraph', items: [ 'NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', '-', 'BidiLtr', 'BidiRtl' ] },
		{ name: 'links', items: [ 'Link', 'Unlink', 'Anchor' ] },
		{ name: 'insert', items: [ 'Image', 'Table', 'HorizontalRule', 'SpecialChar' ] },
		{ name: 'styles', items: [ 'Styles', 'Format' ] },
		{ name: 'tools', items: [ 'Maximize', 'ShowBlocks', 'Source' ] }
	];
	
	// ========================================
	// تنظیمات محتوا - Content Settings
	// ========================================
	config.height = 300;
	config.width = '100%';
	config.enterMode = CKEDITOR.ENTER_P; // استفاده از <p> به جای <div>
	config.shiftEnterMode = CKEDITOR.ENTER_BR; // Shift+Enter برای <br>
	config.autoParagraph = true; // اضافه کردن خودکار <p>
	config.allowedContent = true; // اجازه تمام محتوا (برای محیط درمانی)
	
	// ========================================
	// تنظیمات CSS برای محتوا - Content CSS
	// ========================================
	config.contentsCss = [
		'body { direction: rtl; text-align: right; font-family: Tahoma, Arial, sans-serif; font-size: 14px; line-height: 1.6; }',
		'p { margin: 0 0 1em 0; }',
		'h1, h2, h3, h4, h5, h6 { margin: 1em 0 0.5em 0; font-weight: bold; }',
		'ul, ol { margin: 0.5em 0; padding-right: 2em; }',
		'table { border-collapse: collapse; width: 100%; margin: 1em 0; }',
		'table td, table th { border: 1px solid #ddd; padding: 8px; text-align: right; }'
	];

	// ========================================
	// تنظیمات تصویر - Image Settings
	// ========================================
	config.image_previewText = ' '; // متن پیش‌نمایش تصویر
	config.filebrowserImageUploadUrl = ''; // آپلود تصویر (در صورت نیاز تنظیم شود)
	
	// ========================================
	// تنظیمات لینک - Link Settings
	// ========================================
	config.linkShowTargetTab = true; // نمایش تب target
	config.linkShowAdvancedTab = true; // نمایش تب پیشرفته
	
	// ========================================
	// تنظیمات جدول - Table Settings
	// ========================================
	config.table_defaultContentWidth = '100%';
	
	// ========================================
	// تنظیمات Clipboard - Clipboard Settings
	// ========================================
	config.clipboard_handleImages = false; // غیرفعال کردن مدیریت تصاویر clipboard
	config.pasteFromWordRemoveFontStyles = false; // نگه داشتن استایل‌های فونت از Word
	config.pasteFromWordRemoveStyles = false; // نگه داشتن استایل‌ها از Word
	
	// ========================================
	// تنظیمات امنیتی - Security Settings
	// ========================================
	config.removePlugins = 'elementspath,exportpdf'; // حذف پلاگین‌های غیرضروری
	config.versionCheck = false; // غیرفعال کردن بررسی نسخه
	
	// ========================================
	// تنظیمات دیگر - Other Settings
	// ========================================
	config.resize_enabled = true; // امکان تغییر اندازه
	config.format_tags = 'p;h1;h2;h3;h4;h5;h6;pre'; // تگ‌های فرمت موجود
	
	// ========================================
	// تنظیمات برای محیط درمانی - Medical Environment
	// ========================================
	// این تنظیمات برای محیط درمانی بهینه شده‌اند
	// و از بهترین روش‌های استفاده از CKEditor در پروژه‌های فارسی پشتیبانی می‌کنند
};
