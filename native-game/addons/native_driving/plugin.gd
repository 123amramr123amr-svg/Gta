@tool
extends EditorPlugin

class DrivingExporter extends EditorExportPlugin:
	func _get_name() -> String:
		return "NativeDrivingExporter"

	func _export_begin(features: PackedStringArray, _is_debug: bool, _path: String, _flags: int) -> void:
		if features.has("android"):
			for abi in ["arm64-v8a", "x86_64"]:
				var lib := "res://native/bin/%s/libdriving.so" % abi
				if FileAccess.file_exists(lib):
					add_shared_object(lib, PackedStringArray([abi]), "")

var exporter: DrivingExporter

func _enter_tree() -> void:
	exporter = DrivingExporter.new()
	add_export_plugin(exporter)

func _exit_tree() -> void:
	remove_export_plugin(exporter)
