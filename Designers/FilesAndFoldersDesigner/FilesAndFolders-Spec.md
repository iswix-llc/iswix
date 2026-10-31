# Files and Folders Designer: Specification

Status: written from the designer's code (`Designers/FilesAndFoldersDesigner`) as of 2026-10-31. It records what the designer does, with the rules the code follows, so that behavior changes are deliberate. Where a rule came from the owner rather than the code, it says so. Items marked **Open** are not specified by the code and need a decision.

## 1. Purpose

Edits the file layout of a WiX package: which source files install into which folders of the target computer. It works on one component group of the open `.wxs` document through `DocumentManager`. The user sees the folders on the source side and the folders of the destination computer, and drags files and folders between them; the designer writes the `Component`, `File` and `CreateFolder` elements.

## 2. Documents it edits

- A WiX fragment or package: the document root is `Wix`, and its first child is `Fragment` or `Package` (`IsValidDocument`).
- The file must be saved first. For an unsaved document the designer warns "Please save your file before using Files and Folders" and does not load, because source paths are relative to the file.
- Edits are batched (`DocumentManager.Batch()`), so a drop of thousands of files notifies subscribers and compares the document once.

## 3. Component groups

1. The designer shows one component group at a time. It looks for a group in each `Fragment` of the file and in the `Package`, and shows, in this order: the group asked for, the group whose Id is the file's name (ignoring case), then the first group.
2. **Selector bar** above everything: a drop-down of the file's groups, then the groups of the project's other `.wxs` files with the file's name after the Id, then **New...** and **Delete**.
3. Choosing a group of another file opens that file with the group selected and returns this designer's list to its own group.
4. **New...** asks for the Id and whether to create a new `.wxs` file for the group (checked by default, recommended).
   - The Id must start with a letter or underscore and contain only letters, digits, underscores and periods. A group may be created in a new file only if its Id does not end with a period and `<Id>.wxs` does not exist next to the current file.
   - In the same file: a new `Fragment` holds the group. If the group Id exists, the designer refuses.
   - In a new file: `<Id>.wxs` is written next to the current file, containing a `SourceDir` define and a `Fragment` with the empty group, and then opened.
5. **Delete** removes the group with its components, and its `Fragment` if that holds nothing else. It first confirms, naming the number of components and, when some of them have a service (`ServiceInstall` or `ServiceControl`), how many. It lists up to five files that reference the group by name. Those references are not removed.
6. The group's delete prompt also says how many of its components have registry entries, which go with it.
7. While the file has no group the designer shows "This file has no component groups. Click New... to create one." in place of the folders and files, and the filters and component rules are hidden.
8. Files and folders are added only to a group the file has. A user creates one first.

## 4. Layout

- **Source** (left): "Source Files". A tree of folders under the source directory, a list of the selected folder's files, with columns File Name, Extension, Size, Last Modified, Source Path (full path, hidden), sortable by column. Right-click gives Refresh and Expand All, Collapse All.
- **Destination** (right): "Destination Files". The tree's root is "Destination Computer"; its children are the directories of the components, shown as `[INSTALLFOLDER]` and so on, with their subfolders below. The list shows the selected folder's files with File Name, Extension, File Path (the `Source`) and Key ("Key File" when the file is the component's key path). Sortable.
- On loading, the destination tree is expanded and `[INSTALLLOCATION]` is selected, else the root.
- Directories whose Id is a system folder property (the `SystemFolderProperty` names) get a blue folder icon.
- **Filters:** Include and Exclude text boxes, patterns separated by `;`. A file shows (and is added) if it matches an Include pattern and no Exclude pattern.
- **Component Rules:** One To One or One To Many (section 6).
- A destination file whose source is not on disk shows in red with a deleted-file icon.

## 5. The source directory

- The folder the sources are under comes from a `SourceDir` define at the top of the file (`<?define SourceDir="..."?>`). Without one it is `..\deploy`, relative to the file.
- A folder a file names is created if missing. The default folder is not created merely by looking at the file.
- If the folder cannot be used the designer warns "An error occurred trying to use '<path>' as the SourceDir. Defaulting to document directory." and uses the document's directory.
- Sources are written as `$(var.SourceDir)\<relative path>`. The define is added when files are first added to a group.

## 6. How files become components

A folder of the destination is the set of components with the same `Directory` and `Subdirectory` (the subfolder path, backslash-separated) **that the designer shows**: those with a `File` or a `CreateFolder`. A component with neither (registry values alone, say, from the Registry designer, in the same group) is not a folder's component: the designer does not show it, count it, prune around it, find it when looking for a folder's component, or delete it when a folder is deleted, whatever its `Directory`. `Destination Computer\[INSTALLFOLDER]\bin\x64` is Directory `INSTALLFOLDER`, Subdirectory `bin\x64`.

### 6.1 Adding files

- A file already in the folder (same folder and `Source`) is not added again.
- **Program files get their own component:** `.dll`, `.exe`, `.ocx`, `.chm`, `.hlp`, or any file at all while the rules are One To One. The component has the folder as Directory and Subdirectory, and the file with `KeyPath="yes"`.
- **Other files (One To Many)** go into the folder's own component, the folder's component with `KeyPath` set. If it has none, a new one is made.
- A folder's own component has an Id `owd` plus the SHA-256 hash of the group's Id and the folder path (so the same folder differs between groups), a new GUID, `KeyPath="yes"` and a `CreateFolder`.
- When a file is added to a folder's own component the `CreateFolder` is removed (once is enough).
- An empty folder is represented by a component with just a `CreateFolder`. That empty component is removed ("pruned") once the folder has something else, and restored ("unpruned") when the folder is left with nothing. "Something else" means a component in the folder or in a folder below it, matched on whole path parts, so `bin` and `bin2` are different folders.
- After adding, components and the files in them are put in IsWiX's canonical order. The group is sorted once, after a whole drop, not per file.

### 6.2 Dropping a folder

- Dropping a source folder adds its files and all subfolders' files, applying the filters. A folder with no files and no subfolders adds an empty folder (Source ".").
- The root node of the source tree cannot be dropped ("Copying the root node of the source tree is not allowed...").
- Drops go onto a folder, not onto "Destination Computer" ("Drop the items onto a folder").

### 6.3 Removing files (the file list)

- **Remove From Project** (menu or Delete key) asks "Do you really want to delete the selected file(s)?" and removes the `File` elements. Nothing is deleted from disk.
- A component left with no files is removed whole, unless it still holds something that is not a file or a folder to create (registry values, an environment variable, and so on): that stays. Such a component may then have no KeyPath, which the other designers' validation flags.
- **Service key files (owner requirement):** if the file removed is the component's key file (`KeyPath="yes"`) and the component has a `ServiceInstall` or `ServiceControl` child, the whole component is removed, with any other files in it. Without its key file the service has nothing to install or control. The confirmation says how many selected files are such key files, and that their components are removed whole with the service and any other files in them.
- A folder left with no component gets its empty component back.

### 6.4 Folders

- **Create New Folder** (context menu, or Insert) adds a child folder named "New Folder" (numbered when the name is taken), in label-edit mode. It cannot be added at "Destination Computer".
- **Rename Folder** edits the label. The components of the folder and its subfolders move to the new name. A component Id the designer generated (`owd`/`owc` plus 64 hex digits) is regenerated for the new path; one the user set by hand is kept. A rename to a name that exists is refused. The `Destination Computer` node and top-level directory nodes cannot be renamed ("You cannot rename this folder!").
- **Delete** (Remove From Project, or the Delete key) removes the folder's components and its subfolders'. The Delete key always asks for confirmation; the menu command asks only when the folder (or a folder below it) holds components with a service or registry entries. Either confirmation says how many such components go with the folder. Components the designer does not show (section 6) are not removed. The "Destination Computer" node cannot be deleted. The parent folder gets its empty component back if it is left with nothing.

## 7. Component rules

- Stored as a `ComponentRules` define in the file (`OneToOne` or `OneToMany`), defaulting to One To One when absent.
- They apply when files are added. Changing them does not rewrite existing components.

## 8. Behaviors that follow from the code

- Matching of file and folder names against the destination is case-sensitive for `Directory` and `Subdirectory`, case-insensitive in the tree's lookup table.
- The same file can be in a folder once, but the same source can be added to different folders.
- Components keep the Ids they have; the designer generates an Id only for components it creates.
- Reloading after a text edit keeps the selected group.

## 9. Sharing a group with the Registry designer

See the Registry designer's spec (section 4). Files and Folders owns the components that have a `File` or a `CreateFolder`; the Registry designer's own components have the Id prefix `owr`. Files and Folders' generated-Id rename only touches `owd` and `owc` Ids. Where a registry entry is attached to a component Files and Folders owns, deleting the folder, the group, or a service's key file removes it with the component, and the prompts say so.

## 10. Open items

1. **Open:** changing Component Rules does not offer to convert the existing components.
2. **Open:** dragging within the destination (a Move from one destination folder to another) is wired (`DragSourceName` "lvDestination" gives a Move effect), but its behavior should be confirmed and written down if it is meant to be supported.
3. **Open:** `GetNextSubDirectoryName` counts any folder whose name starts with "New Folder" when numbering a new folder, so "New Folder Backup" makes the next one "New Folder (1)". Harmless, but it is a prefix match.
