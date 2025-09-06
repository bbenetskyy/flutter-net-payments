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
        effectivePermissions: $checkedConvert(
          'effectivePermissions',
          (v) => (v as num?)?.toInt(),
        ),
        dobHash: $checkedConvert('dobHash', (v) => v as String?),
        verificationStatus: $checkedConvert(
          'verificationStatus',
          (v) => (v as num?)?.toInt(),
        ),
        createdAt: $checkedConvert('createdAt', (v) => v as String?),
      );
      return val;
    });

Map<String, dynamic> _$UserResponseToJson(UserResponse instance) =>
    <String, dynamic>{
      'id': ?instance.id,
      'email': ?instance.email,
      'displayName': ?instance.displayName,
      'role': ?instance.role,
      'effectivePermissions': ?instance.effectivePermissions,
      'dobHash': ?instance.dobHash,
      'verificationStatus': ?instance.verificationStatus,
      'createdAt': ?instance.createdAt,
    };
