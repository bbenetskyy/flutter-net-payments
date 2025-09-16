//in c# it's
//    u.Id,
//                     u.Email,
//                     u.DisplayName,
//                     Role = u.Role.Name,
//                     EffectivePermissions = (long)(u.OverridePermissions ?? u.Role.Permissions),
//                     DobHash = u.DobHash,
//                     u.VerificationStatus,
//                     u.CreatedAt

import 'package:json_annotation/json_annotation.dart';
part 'user_response.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class UserResponse {
  /// Returns a new [UserResponse] instance.
  UserResponse({
    this.id,
    this.email,
    this.displayName,
    this.roleId,
    this.roleName,
    this.effectivePermissions,
    this.dobHash,
    this.verificationStatus,
    this.createdAt,
  });

  @JsonKey(name: r'id', required: false, includeIfNull: false)
  final String? id;

  @JsonKey(name: r'email', required: false, includeIfNull: false)
  final String? email;

  @JsonKey(name: r'displayName', required: false, includeIfNull: false)
  final String? displayName;

  @JsonKey(name: r'roleId', required: false, includeIfNull: false)
  final String? roleId;

  @JsonKey(name: r'roleName', required: false, includeIfNull: false)
  final String? roleName;

  @JsonKey(name: r'effectivePermissions', required: false, includeIfNull: false)
  final int? effectivePermissions;

  @JsonKey(name: r'dobHash', required: false, includeIfNull: false)
  final String? dobHash;

  @JsonKey(name: r'verificationStatus', required: false, includeIfNull: false)
  final int? verificationStatus;

  @JsonKey(name: r'createdAt', required: false, includeIfNull: false)
  final String? createdAt;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is UserResponse &&
          other.id == id &&
          other.email == email &&
          other.displayName == displayName &&
          other.roleId == roleId &&
          other.roleName == roleName &&
          other.effectivePermissions == effectivePermissions &&
          other.dobHash == dobHash &&
          other.verificationStatus == verificationStatus &&
          other.createdAt == createdAt;

  @override
  int get hashCode =>
      (id == null ? 0 : id.hashCode) +
      (email == null ? 0 : email.hashCode) +
      (displayName == null ? 0 : displayName.hashCode) +
      (roleId == null ? 0 : roleId.hashCode) +
      (effectivePermissions == null ? 0 : effectivePermissions.hashCode) +
      (dobHash == null ? 0 : dobHash.hashCode) +
      (verificationStatus == null ? 0 : verificationStatus.hashCode) +
      (createdAt == null ? 0 : createdAt.hashCode);

  factory UserResponse.fromJson(Map<String, dynamic> json) {
    // Normalize role value: backend may return either:
    //  - roleName/roleId as flat properties (ideal)
    //  - a nested `role` object { id, name, ... }
    //  - a plain `role` string
    // Make sure roleName and roleId are populated accordingly before delegating
    // to the generated constructor.
    final data = Map<String, dynamic>.from(json);

    final roleValue = data['role'];
    // If backend returned a nested role map, extract both name and id.
    if (roleValue is Map) {
      final dynamic name = roleValue['name'] ?? roleValue['title'] ?? roleValue['code'];
      final dynamic id = roleValue['id'] ?? roleValue['roleId'];
      if (data['roleName'] == null && name != null) {
        data['roleName'] = name.toString();
      }
      if (data['roleId'] == null && id != null) {
        data['roleId'] = id.toString();
      }
    } else if (roleValue is String) {
      // If a plain string was provided for role, map it to roleName if missing.
      data['roleName'] ??= roleValue;
    }

    // Some backends may use different casing/keys. Map common alternates.
    if (data['role_name'] != null && data['roleName'] == null) {
      data['roleName'] = data['role_name'];
    }
    if (data['role_id'] != null && data['roleId'] == null) {
      data['roleId'] = data['role_id'];
    }

    return _$UserResponseFromJson(data);
  }
  Map<String, dynamic> toJson() => _$UserResponseToJson(this);

  static List<UserResponse> listFromJson(dynamic data) {
    if (data is List) {
      return data.map((e) => UserResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['items'] is List) {
      return (data['items'] as List).map((e) => UserResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['data'] is List) {
      return (data['data'] as List).map((e) => UserResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map<String, dynamic>) {
      return [UserResponse.fromJson(data)];
    }
    return const [];
  }

  @override
  String toString() {
    return toJson().toString();
  }
}
