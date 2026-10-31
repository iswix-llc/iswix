# Registry Designer: Requirements Spec

Status: agreed, from the requirements interview of 2026-10-31 and the owner's review of the proposals the same day.

## 1. Purpose

A designer in IsWiX for Visual Studio for authoring the registry entries of a WiX package, alongside the existing designers (Files and Folders, Services, Shortcuts, and so on). It edits the open `.wxs` document through `DocumentManager`, and its edits are written to the text buffer like every other designer's.

## 2. Scope

### 2.1 In scope (v1)

| Area | Requirement |
|---|---|
| Elements | `RegistryKey` and `RegistryValue` (nested in a key, and standalone with `Root`/`Key` on the value); `RemoveRegistryKey`; `RemoveRegistryValue` |
| Value types | The types the IsWiX model has for `RegistryValue`: String, Integer (a DWORD), Binary, Expandable and MultiString. There is no QWORD type (see section 5). Values may contain `[Property]` tokens |
| Advanced attributes | Registry view (32-bit or 64-bit), which is the component's `Bitness`, checked against the IsWiX model: `RegistryKey`, `RegistryValue` and the Remove elements have no view attribute of their own; `ForceCreateOnInstall` and `ForceDeleteOnUninstall` on keys (`RegistryKey` only); `Action` (on values and `RemoveRegistryKey`; a key's own `Action` is deprecated and is never written); Permissions on keys and values, the Util extension's `PermissionEx` and a core `PermissionEx` with a security descriptor; the plain `Permission` (the LockPermissions table) is not offered, because its behavior surprises; `DisableRegistryReflection` on the component; a Condition on the component (the entries have none) |
| Components | Auto-managed components plus attaching entries to any component (section 4) |
| Validation | Section 6 |
| Import and export | `.reg` file import and `.reg` export (section 7) |
| Source view | A read-only view of this computer's registry above the destination, to drag keys and values from (section 10) |

### 2.2 Out of scope (v1)

- `RegistrySearch` (locating existing registry data into properties).

## 3. Layout

**What it shows.** One component group at a time, chosen with the group picker (section 4.1), as Files and Folders does. Registry entries in other groups, or in components outside any group, are not shown.

- Left: a tree of hive and key paths (`HKLM\Software\...`), including keys that exist only as a `RegistryKey` element, a `RemoveRegistryKey`, or the `Key` of a standalone value.
- Right: a grid of the values of the selected key (name, type, value, and the advanced attributes).
- The grid is editable in place. Adding, deleting and editing entries are undoable as one editor undo step each, like the other designers' edits.
- Like the other designers, the selection is linked to the XML: selecting an entry moves the caret to its element, and moving the caret in the XML selects the entry.

## 4. Components and component groups

1. **Group home.** Auto-managed registry components live in a component group, and they coexist in a group that Files and Folders also writes. The designer has a component group picker like Files and Folders, defaulting to the group Files and Folders is showing.
2. **Auto-managed component.** When the user adds an entry without choosing a component, the designer puts it in one shared registry component for the group. The designer creates that component if the group has none and gives it an Id derived from the group's. A component in a group needs a `Directory`, so the designer **asks the user to pick one** (section 4.7) before it creates the component.
3. **Attach to any component.** A user can tie an entry to any component in the document, including components Files and Folders created, and the entry is added under that component. The entry can be moved to a different component afterwards.
4. **Scope and view split.** Entries are split into separate auto-managed components by scope (per-user and per-machine) and by registry view (the component's `Bitness`: checked against the IsWiX model, no registry element but `RegistrySearch` has a view attribute of its own), because one component cannot mix them without causing ICE38 and ICE43 problems and bitness conflicts. Each such component keeps its own KeyPath.
5. Deleting the last entry of an auto-managed component removes the component. A component that other tools also use (it has files, services, and so on) is not removed.
6. **Sharing a group with Files and Folders.** Both designers edit components in the same group, so each must leave the other's alone:
   - Auto-managed registry components have an Id with the prefix `owr` plus a hash of the group Id and the scope and view. Files and Folders' own ids use `owd` (and the older `owc`), so a rename in Files and Folders never regenerates a registry component's Id.
   - A component in a group needs a `Directory`, so a registry component has one, chosen by the user (section 4.7). Files and Folders shows, counts, prunes and deletes only components that have a `File` or a `CreateFolder`. A registry-only component is invisible to it whatever its `Directory`, so a folder delete or an empty-folder rule never touches it.
   - Registry values attached to a component Files and Folders owns (a file's component) are deleted with that component when its folder or group is deleted, or when a service's key file is deleted. Files and Folders says so in those prompts. Deleting the last file of such a component does not delete the component while it still has registry entries.
   - A component with registry entries and no KeyPath is not flagged: that is not worth a warning.

7. **Picking the Directory.** The first time the designer needs to create an auto-managed registry component, and whenever it creates one for a scope and view that has none, it shows a dialog asking which `Directory` the component uses.
   - The list holds the directories the document knows: the `Directory` and `StandardDirectory` elements of the file, the `Directory` values the group's existing components use, and the standard directory names WiX defines (such as `ProgramFiles6432Folder` and `TARGETDIR`).
   - The directory that Files and Folders' components in the group use most often is selected by default. If the group has none, `TARGETDIR` is.
   - When every component being made is per-user (HKCU), `LocalAppDataFolder` is selected instead: a per-user component belongs in the user's profile. When a dialog serves both kinds at once (an import with HKLM and HKCU entries), the per-machine choice above is selected.
   - The user can type an Id that is not in the list (a directory defined in another file of the project). The dialog checks it is a valid identifier.
   - Cancelling creates nothing, and the entry is not added.
   - The choice is kept in the group's component, not in a setting: later entries for the same scope and view reuse that component, so the dialog does not appear again for it. Changing the `Directory` of the component afterwards is a property of the component in the designer, and moves nothing else.
   - A user who attaches an entry to an existing component (4.3) is not asked: that component already has a `Directory`.

## 5. Value editing

**Removal options.** A `RemoveRegistryKey` entry shows its `Action` as a choice the user makes in the grid: remove on install, or remove on uninstall (the model's `RemoveRegistryKeyAction` values). A `RemoveRegistryValue` entry has no action in the model, so none is offered for it. When the user adds a removal entry, the choice is empty until they set it.

**Value types and QWORD.** QWORD is not supported. Windows Installer's Registry table has no 64-bit integer type, and the model's `RegistryValueType` has none either, so the grid does not offer QWORD. Import does not convert a `REG_QWORD` value: it is skipped and listed in the import report as unsupported. This replaces the earlier requirement to support QWORD.

**KeyPath.** The grid has a KeyPath column. The user marks which value is the component's KeyPath; the designer does not choose one. A component with registry entries and no KeyPath is not flagged. Deleting the KeyPath value does not move it to another value.

**Hives.** The Root choices are `HKCR`, `HKCU`, `HKLM`, `HKU` and `HKMU`. `HKMU` (current user or machine, by the install's `ALLUSERS` scope) is treated as per-machine for the scope split in section 4.4, so its entries share a component with `HKLM` entries, and the tree notes that it follows `ALLUSERS`.

**Component properties.** Settings that belong to the component, not to an entry, are edited on the component's properties, which show when a component is selected in the tree:
- Registry view (`Bitness`: default, always 32-bit, always 64-bit).
- `DisableRegistryReflection`, an optional yes/no checkbox, unset by default. It stops Windows from mirroring registry entries between the 32-bit and 64-bit views; leave it unset unless the entries need it. No validation.
- The component's Condition.
- Which value is the KeyPath is set on the value (section 5).
- Its `Directory` (section 4.7).


- Type-specific editors in the grid: text for String; numeric entry (decimal, with hex display) for Integer (DWORD); a hex editor for Binary; a multi-line editor for MultiString (one string per line, each written as a `MultiStringValue` child element of the value; the value's own `Value` attribute is not used); and text for Expandable.
- `[Property]` tokens are accepted in string-like values.
- `RemoveRegistryKey` and `RemoveRegistryValue` entries show in the same tree and grid with an indication that they delete rather than write.

## 6. Validation

Shown inline, without blocking edits, and cleared as the user fixes them.

- **Required fields:** Root and Key are required on every entry. A value with no name is the key's default value, which is normal and is not flagged.
- **Duplicates:** the same Root, Key and Name (in the same view) appearing more than once.
- **Value format by type:**
  - DWORD in 0 to 4294967295.
  - Binary is valid hex with an even digit count.
  - Property token syntax is balanced.
- **KeyPath rules:**
  - A component has at most one KeyPath.
  - A file KeyPath and a registry KeyPath in one component conflict.
  - A component with a component-level KeyPath (`KeyPath="yes"` on the `Component`, as Files and Folders gives a folder's own component) cannot also have a registry value as its KeyPath.
  - A component with registry entries and no KeyPath is not flagged.
- **Package scope.** "Per-machine" and "per-user" for the warning below come from the package's `Scope` attribute (`perMachine` or `perUser`). Without one, the package is treated as per-machine.
- **Per-user keys in a per-machine package:** a warning on HKCU entries, with a suggested fix (for example, moving them to a separate per-user component), as in section 4.4. The warning is not given for a component that already is what it suggests: only per-user entries, no files, and a registry value as its KeyPath. This includes HKCU values attached to a component that also holds files. Keys kept on their own (a `RegistryKey` with no values) count the same as values, and a component that holds files counts as per-machine, so an HKCU entry beside a file is flagged as mixed scope (ICE57).
- **A per-user component with no registry KeyPath:** a warning (ICE57): without one the component's Directory is its KeyPath, which is per-machine. Fix Components makes one of its values the KeyPath, or adds the value described below.
- **Fix Components:** a button under the problems list, enabled when a component mixes per-user and per-machine entries (or files) has KeyPaths that conflict, or is a per-user component with no registry KeyPath. It asks first, then moves the HKCU values, keys and removals into the group's per-user component (made if there is none, asking for its Directory), gives that component a KeyPath from one of its values if it has none, keeps only the first registry KeyPath of a component with several, and takes a registry KeyPath off a component whose KeyPath is a file or the component itself. A per-user component that has only keys would have its Directory as its KeyPath, which is per-machine (ICE57), so the fix adds a value `installed` = 1 under its first key as the KeyPath and says so in the result. All of it is done in one batch.

Out of scope for validation: flagging `[Property]` references that don't exist in the document.

## 7. Import and export

- **Import `.reg`:** parse a regedit export into keys and values, mapping the value types to section 5's types (a multi-string value becomes one `MultiStringValue` child per string, and export writes them back as one multi-string value), and add them as auto-managed entries (section 4). The user chooses the target component group. Unsupported content is reported, not silently dropped.
- **Brackets.** In a formatted value a `[` starts a property reference, so a `.reg` string containing a literal `[` or `]` is imported with the brackets escaped (`[\[]` and `[\]]`), and export turns the escapes back into brackets. The grid shows the value as it is in the XML, so a user typing a token is typing a token.
- **Export `.reg`:** write the designer's keys and values out as a `.reg` file.
- **Property tokens on export.** A `[Property]` token has no `.reg` equivalent. Export writes the token text as it is and shows a warning listing the values that contain tokens.

## 8. Save behavior

Nothing registry-specific. The designer's edits go through the same path as every other designer, so the save-time indentation enforcement applies to the document.

## 9. Implementation notes

- **Testable model.** Like `ComponentGroupDocument`, the document logic (finding entries, adding, removing, attaching to components, scope and view splitting, validation, `.reg` parse and write) lives in classes that use no Windows Forms or Visual Studio types, so the test project can compile them in and test them with xunit. The UI only calls them.
- **Large imports.** Import and bulk edits use `DocumentManager.Batch()`, append elements and put the group in canonical order once at the end, as Files and Folders does for a drop of thousands of files.

- Project: `Designers/RegistryDesigner`, following the structure of the Services designer.
- The document model works on `XElement` through `DocumentManager`, batching multi-node edits with `Batch()`.
- New elements go in canonical positions through the IsWiX model.
- The designer needs registering with the designer list the same way the others are.

## 10. The window as built

`Registry.cs`, `Labels.cs` and `Dialogs.cs` in `Designers/RegistryDesigner`. The window is code-built Windows Forms (no `.Designer.cs` or `.resx`), themed by the host like the other WinForms designers.

- **Source (above the destination, hidden with the Source Registry toolbar button):** this computer's registry, read-only, as in InstallShield: a tree of "My Computer" with HKCR, HKCU, HKLM and HKU, whose subkeys are read as they are opened, and a list of the selected key's values (Name, Type, Data as regedit shows it; values Windows Installer can't write, such as a QWORD, are shown in grey). A choice picks the 64-bit or the 32-bit registry view to read. A key that can't be read (no permission) shows in grey with the reason.
  - **Dragging:** a key (with everything below it) or selected values drag onto the destination tree and keep their fully qualified path: a key dropped from `HKLM\\Software\\Acme` arrives as `HKLM\\Software\\Acme`, wherever on the tree it is dropped, and the tree then selects it. The drop reads what was dragged and adds it the way importing a .reg file does: values already in the group are left alone, unsupported ones are reported, a key with nothing in it is kept as a key, and a key with more than 5000 values is refused.
  - **"Add dragged entries as":** the registry view the new components are written in: the package's default, or 32-bit or 64-bit. The source's view and the destination's are separate choices, because WiX takes the package's bitness from the build, not the source, and a 64-bit component in a 32-bit package is a Windows Installer error.
  - HKMU isn't a root of the computer, so it isn't in the source.
- **Top:** a component group picker (default: the group named after the file, else the file's first group) and a view filter, then a toolbar: New Key, New Value (a menu), Remove Entry..., Delete, Find..., Source Registry, Import .reg..., Export .reg....
  - **View filter:** shows all entries, only those with problems (their own, or their component's), the per-user (HKCU) or the per-machine ones, or the entries of one component. It limits the tree and the grid; Find clears it if a match is hidden.
  - **New Value menu:** Value... (the general dialog), Default Value, String Value, Binary Value, DWORD Value, Multi-String Value and Expandable String Value, each opening the dialog with that type chosen (a default value has no name to enter).
- **Left:** the tree, modelled on InstallShield's registry editor. "Destination Computer" at the top, the roots HKCR, HKCU, HKLM, HKU and HKMU always under it (so a key can be added to any of them, even an empty one), then the keys, with folder icons. Its menu (right-click, which selects the key under the mouse) and keys:
  - **New Key (Ins):** adds a key under the selected one, named New Key (numbered if taken), with its name open for editing. It is kept as a `RegistryKey` with `ForceCreateOnInstall` set until it has values.
  - **Rename (F2, or edit the label):** gives the key a new name and moves everything at it and below it; refused for an empty name, one with a backslash, or a name another key at that level has.
  - **Copy (Ctrl+C) and Paste (Ctrl+V):** copies the key and what is below it (or, with the grid focused, the selected values) as .reg text on the clipboard, with a comment line saying what it was. A paste of such a copy puts the key, by its name, under the selected key or root, or the values into the selected key; .reg text from anywhere else is pasted at the paths it has. Values already there are left alone and reported.
  - **Delete (Del):** removes the key and everything below it, after confirming how many keys and entries go. The roots can't be deleted.
  - **Import .reg..., Export All..., Export Selected Branch...:** the branch is the selected key and what is below it (or a whole root), with its empty keys.
- **Right, top:** a grid of the selected key's entries: Entry, Name, Type, Value, Action, KeyPath, Component. an icon for the kind of entry (string, DWORD, binary, expandable, multi-string, removal) comes first, and a value with no name shows as "(Default)". Integer data shows as regedit shows it (hex and decimal) and binary data as hex pairs, until the cell is edited, when it shows what is stored. Name, Type, Value, Action, KeyPath and Component are edited in place; a multi-string's strings are edited in a dialog (double-click its Value). Rows with problems show the red error mark with the text; Delete or the context menu removes the selected rows after confirming.
- **Right, bottom:** on the left, the selected entry's component (Directory, registry view, Condition, DisableRegistryReflection), and below it the selected key's options; on the right, the issue list (double-click one to select the entry).
  - **Key options:** for the selected key, the ForceCreateOnInstall and ForceDeleteOnUninstall flags (each yes, no or unset). A key's `Action` is deprecated and can't be used: it is not offered, setting the key's options removes one a file still has, and validation warns about a key that has one. They are kept on a `RegistryKey` element at exactly that key, which is made, in the component that has the key's values or else in the group's own (asking for the Directory if there is none yet), when the first option is set. An unset option is left out of the XML.
  - **Permissions:** a Permissions dialog for the selected key (key panel button, or Permissions... in the tree menu) and for a value (Permissions... in the grid menu). Its list offers two kinds: the Util extension's `PermissionEx` (user, domain, inheritance and the rights), which is what Add makes, and a `PermissionEx` with an SDDL security descriptor and a condition. The plain `Permission` is not offered; one in a file opens as a Util `PermissionEx` and is written as one when the dialog is accepted. The rights offered are the ones a registry key has: generic all, read, write and execute; query value (Read), set value (Write), create subkeys, enumerate subkeys, notify, create link; and delete, read permissions, change permissions, take ownership and synchronize. A permission needs a user (or, for SDDL, a descriptor). Accepting replaces the element's permissions with the list.
- **Find (Ctrl+F, F3):** asks for text, whether to look in key names, value names and value data, and whether to match case; goes to the first match and then to each next one (F3), key before its values, wrapping at the end. "Not found" is said once.
- **Insert Property:** the value dialog's Insert Property... button, and Insert Property... in the grid menu while editing a value, list what a formatted value can refer to: the file's properties and directories, the paths of its files (`[#FileId]`), and the properties and folders Windows Installer defines. It puts the reference, such as `[INSTALLFOLDER]`, in the value at the caret. Whether a reference exists is not checked.
- **Dialogs:** New Value and Remove Entry (root, key, name, type, value or strings, action, which component, and the registry view for the group's own component); the Directory dialog of section 4.7.
- **Linking:** selecting an entry moves the XML caret to its element, and the caret selects the entry, as in the other designers. The window handles any registry element and a `MultiStringValue`.
- **Not done yet:** the group picker does not follow the group Files and Folders is showing (they are separate windows with separate selections); a key to remove has its Action set in the grid, but a removal's root, key and name are not editable in place (delete it and add another).

## 11. Open decisions

- Whether the Directory dialog (section 4.7) offers to remember the choice for later groups. Not decided; until it is, the dialog does not.
