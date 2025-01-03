class_name CommandHandler
extends Node

const INCOMING_MESSAGES_FOLDER := "res://addons/neuro-sdk/messages/incoming/"
var handlers: Array[IncomingMessage] = []

func register_all() -> void:
	var dir := DirAccess.open(INCOMING_MESSAGES_FOLDER)
	if not dir:
		push_error("Could not open websocket messages directory")
		return

	dir.list_dir_begin()
	var file_name := dir.get_next()
	while file_name != "":
		if file_name.ends_with(".remap"):
			file_name = file_name.trim_suffix(".remap")
		if file_name.ends_with(".gd"):
			var script_path := INCOMING_MESSAGES_FOLDER + file_name
			var script = load(script_path)
			if script:
				var node = script.new()
				node.name = file_name
				add_child(node)
				handlers.append(node)
				print("Added websocket message node: %s" % [script_path])
		file_name = dir.get_next()
	dir.list_dir_end()

func handle(command: String, data: IncomingData) -> void:
	for handler in handlers:
		if !handler.can_handle(command):
			continue

		var state := {}

		var validation_result := handler.validate(command, data, state)
		if !validation_result.successful:
			push_warning("Received unsuccessful execution result when handling a message")
			push_warning(validation_result.message)

		handler.report_result(state, validation_result)

		if validation_result.successful:
			handler.execute(state)
