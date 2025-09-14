//in C# its
//public record RoleResponse(
//     Guid Id,
//     string Name,
//     UserPermissions Permissions,
//     DateTime CreatedAt,
//     int UsersCount);

import 'package:json_annotation/json_annotation.dart';

import '../user_permissions.dart';

part 'role_response.g.dart';

// Custom converters for UserPermissions flags on responses
Set<UserPermissions> _permissionsFromJson(Object? json) {
  final intMask = switch (json) {
    int v => v,
    String s => int.tryParse(s) ?? 0,
    _ => 0,
  };
  return UserPermissionsConverter.fromBitMask(intMask);
}

Object _permissionsToJson(Set<UserPermissions> value) {
  return UserPermissionsConverter.toBitMask(value);
}

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class RoleResponse {
  /// Returns a new [RoleResponse] instance.
  RoleResponse({
    required this.id,
    required this.name,
    required this.permissions,
    required this.createdAt,
    required this.usersCount,
  });

  @JsonKey(name: r'id', required: true, includeIfNull: false)
  final String id;

  @JsonKey(name: r'name', required: true, includeIfNull: false)
  final String name;

  // Flags set parsed from backend bitmask
  @JsonKey(
    name: r'permissions',
    required: true,
    includeIfNull: false,
    fromJson: _permissionsFromJson,
    toJson: _permissionsToJson,
  )
  final Set<UserPermissions> permissions;

  @JsonKey(name: r'createdAt', required: true, includeIfNull: false)
  final DateTime createdAt;

  @JsonKey(name: r'usersCount', required: true, includeIfNull: false)
  final int usersCount;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is RoleResponse &&
          other.id == id &&
          other.name == name &&
          other.permissions == permissions &&
          other.createdAt == createdAt &&
          other.usersCount == usersCount;

  @override
  int get hashCode => id.hashCode + name.hashCode + permissions.hashCode + createdAt.hashCode + usersCount.hashCode;

  factory RoleResponse.fromJson(Map<String, dynamic> json) => _$RoleResponseFromJson(json);

  Map<String, dynamic> toJson() => _$RoleResponseToJson(this);

  static List<RoleResponse> listFromJson(dynamic data) {
    if (data is List) {
      return data.map((e) => RoleResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['items'] is List) {
      return (data['items'] as List).map((e) => RoleResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['data'] is List) {
      return (data['data'] as List).map((e) => RoleResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map<String, dynamic>) {
      return [RoleResponse.fromJson(data)];
    }
    return const [];
  }

  @override
  String toString() {
    return toJson().toString();
  }
}
