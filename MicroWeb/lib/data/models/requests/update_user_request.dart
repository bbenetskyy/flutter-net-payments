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

// DateOnly converters (yyyy-MM-dd) for backend compatibility
DateTime? _dateOnlyFromJson(Object? v) {
  if (v == null) return null;
  if (v is String) {
    try {
      // Accept both yyyy-MM-dd and full ISO strings
      return DateTime.parse(v);
    } catch (_) {
      return null;
    }
  }
  return null;
}

Object? _dateOnlyToJson(DateTime? dt) {
  if (dt == null) return null;
  final y = dt.year.toString().padLeft(4, '0');
  final m = dt.month.toString().padLeft(2, '0');
  final d = dt.day.toString().padLeft(2, '0');
  return '$d-$m-$y';
}

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class UpdateUserRequest {
  /// Returns a new [UpdateUserRequest] instance.
  UpdateUserRequest({this.displayName, this.roleId, this.dateOfBirth, this.overridePermissions});

  @JsonKey(name: r'displayName', required: false, includeIfNull: false)
  final String? displayName;

  @JsonKey(name: r'roleId', required: false, includeIfNull: false)
  final String? roleId;

  // Backend expects DateOnly (yyyy-MM-dd). Serialize as such and accept either
  // full ISO8601 or yyyy-MM-dd on deserialization.
  @JsonKey(
    name: r'dateOfBirth',
    required: false,
    includeIfNull: false,
    // fromJson: _dateOnlyFromJson,
    // toJson: _dateOnlyToJson,
  )
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
