// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'admin_create_user_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AdminCreateUserRequest _$AdminCreateUserRequestFromJson(
  Map<String, dynamic> json,
) => $checkedCreate('AdminCreateUserRequest', json, ($checkedConvert) {
  final val = AdminCreateUserRequest(
    email: $checkedConvert('email', (v) => v as String?),
    displayName: $checkedConvert('displayName', (v) => v as String?),
    desiredRoleId: $checkedConvert('desiredRoleId', (v) => v as String?),
    dateOfBirth: $checkedConvert(
      'dateOfBirth',
      (v) => v == null ? null : DateTime.parse(v as String),
    ),
  );
  return val;
});

Map<String, dynamic> _$AdminCreateUserRequestToJson(
  AdminCreateUserRequest instance,
) => <String, dynamic>{
  'email': ?instance.email,
  'displayName': ?instance.displayName,
  'desiredRoleId': ?instance.desiredRoleId,
  'dateOfBirth': ?instance.dateOfBirth?.toIso8601String(),
};
