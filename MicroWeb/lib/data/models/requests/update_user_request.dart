//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';
import 'package:micro_web/data/models/user_permissions.dart';

part 'update_user_request.g.dart';

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
class UpdateUserRequest {
  /// Returns a new [UpdateUserRequest] instance.
  UpdateUserRequest({this.displayName, this.roleId, this.dateOfBirth, this.overridePermissions});

  @JsonKey(name: r'displayName', required: false, includeIfNull: false)
  final String? displayName;

  @JsonKey(name: r'roleId', required: false, includeIfNull: false)
  final String? roleId;

  @JsonKey(name: r'dateOfBirth', required: false, includeIfNull: false)
  final DateTime? dateOfBirth;

  // Flags: backend expects combined integer bitmask of UserPermissions
  @JsonKey(
    name: r'overridePermissions',
    required: false,
    includeIfNull: false,
    fromJson: _permissionsFromJson,
    toJson: _permissionsToJson,
  )
  final Set<UserPermissions>? overridePermissions;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is UpdateUserRequest &&
          other.displayName == displayName &&
          other.roleId == roleId &&
          other.dateOfBirth == dateOfBirth &&
          other.overridePermissions == overridePermissions;

  @override
  int get hashCode =>
      (displayName == null ? 0 : displayName.hashCode) +
      (roleId == null ? 0 : roleId.hashCode) +
      (dateOfBirth == null ? 0 : dateOfBirth.hashCode) +
      overridePermissions.hashCode;

  factory UpdateUserRequest.fromJson(Map<String, dynamic> json) => _$UpdateUserRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateUserRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
