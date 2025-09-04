// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'user_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UserResponse _$UserResponseFromJson(Map<String, dynamic> json) =>
    $checkedCreate('UserResponse', json, ($checkedConvert) {
      final val = UserResponse(
        id: $checkedConvert('id', (v) => v as String?),
        email: $checkedConvert('email', (v) => v as String?),
        displayName: $checkedConvert('displayName', (v) => v as String?),
        role: $checkedConvert('role', (v) => v as String?),
        effectivePermissions: $checkedConvert('effectivePermissions', (v) => (v as num?)?.toInt()),
        dobHash: $checkedConvert('dobHash', (v) => v as String?),
        verificationStatus: $checkedConvert('verificationStatus', (v) => v as String?),
        createdAt: $checkedConvert('createdAt', (v) => v as String?),
      );
      return val;
    });

Map<String, dynamic> _$UserResponseToJson(UserResponse instance) {
  final val = <String, dynamic>{};

  void writeNotNull(String key, dynamic value) {
    if (value != null) val[key] = value;
  }

  writeNotNull('id', instance.id);
  writeNotNull('email', instance.email);
  writeNotNull('displayName', instance.displayName);
  writeNotNull('role', instance.role);
  writeNotNull('effectivePermissions', instance.effectivePermissions);
  writeNotNull('dobHash', instance.dobHash);
  writeNotNull('verificationStatus', instance.verificationStatus);
  writeNotNull('createdAt', instance.createdAt);
  return val;
}
