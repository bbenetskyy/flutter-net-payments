//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';
import 'package:micro_web/data/models/user_permissions.dart';

part 'update_role_request.g.dart';

// Custom converters for UserPermissions flags
Set<UserPermissions>? _permissionsFromJson(Object? json) {
  if (json == null) return null;
  final intMask = switch (json) {
    int v => v,
    String s => int.tryParse(s) ?? 0,
    _ => 0,
  };
  return UserPermissionsConverter.fromBitMask(intMask);
}

Object? _permissionsToJson(Set<UserPermissions>? value) {
  if (value == null) return null;
  return UserPermissionsConverter.toBitMask(value);
}

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class UpdateRoleRequest {
  /// Returns a new [UpdateRoleRequest] instance.
  UpdateRoleRequest({this.name, this.permissions});

  @JsonKey(name: r'name', required: false, includeIfNull: false)
  final String? name;

  // Flags: backend expects combined integer bitmask of UserPermissions
  @JsonKey(
    name: r'permissions',
    required: false,
    includeIfNull: false,
    fromJson: _permissionsFromJson,
    toJson: _permissionsToJson,
  )
  final Set<UserPermissions>? permissions;

  @override
  bool operator ==(Object other) =>
      identical(this, other) || other is UpdateRoleRequest && other.name == name && other.permissions == permissions;

  @override
  int get hashCode => (name == null ? 0 : name.hashCode) + permissions.hashCode;

  factory UpdateRoleRequest.fromJson(Map<String, dynamic> json) => _$UpdateRoleRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateRoleRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
