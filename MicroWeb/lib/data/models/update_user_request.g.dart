// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'update_user_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UpdateUserRequest _$UpdateUserRequestFromJson(Map<String, dynamic> json) =>
    $checkedCreate('UpdateUserRequest', json, ($checkedConvert) {
      final val = UpdateUserRequest(
        displayName: $checkedConvert('displayName', (v) => v as String?),
        roleId: $checkedConvert('roleId', (v) => v as String?),
        dateOfBirth: $checkedConvert(
          'dateOfBirth',
          (v) => v == null ? null : DateTime.parse(v as String),
        ),
        overridePermissions: $checkedConvert(
          'overridePermissions',
          (v) => _permissionsFromJson(v),
        ),
      );
      return val;
    });

Map<String, dynamic> _$UpdateUserRequestToJson(UpdateUserRequest instance) =>
    <String, dynamic>{
      'displayName': ?instance.displayName,
      'roleId': ?instance.roleId,
      'dateOfBirth': ?instance.dateOfBirth?.toIso8601String(),
      'overridePermissions': ?_permissionsToJson(instance.overridePermissions),
    };
