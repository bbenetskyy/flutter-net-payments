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
    this.role,
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

  @JsonKey(name: r'role', required: false, includeIfNull: false)
  final String? role;

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
          other.role == role &&
          other.effectivePermissions == effectivePermissions &&
          other.dobHash == dobHash &&
          other.verificationStatus == verificationStatus &&
          other.createdAt == createdAt;

  @override
  int get hashCode =>
      (id == null ? 0 : id.hashCode) +
      (email == null ? 0 : email.hashCode) +
      (displayName == null ? 0 : displayName.hashCode) +
      (role == null ? 0 : role.hashCode) +
      (effectivePermissions == null ? 0 : effectivePermissions.hashCode) +
      (dobHash == null ? 0 : dobHash.hashCode) +
      (verificationStatus == null ? 0 : verificationStatus.hashCode) +
      (createdAt == null ? 0 : createdAt.hashCode);

  factory UserResponse.fromJson(Map<String, dynamic> json) {
    // Normalize role value: backend may return a nested object for role, while
    // this model expects a String (e.g., role name). We extract a reasonable
    // string (name/title/id) if a map is provided.
    final data = Map<String, dynamic>.from(json);
    final roleValue = data['role'];
    if (roleValue is Map) {
      // Try common keys in order of preference
      final dynamic name = roleValue['name'] ?? roleValue['title'] ?? roleValue['code'] ?? roleValue['id'];
      if (name != null) {
        data['role'] = name.toString();
      } else {
        // Fallback to a simple string representation
        data['role'] = roleValue.toString();
      }
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
