// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'update_role_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UpdateRoleRequest _$UpdateRoleRequestFromJson(Map<String, dynamic> json) =>
    $checkedCreate('UpdateRoleRequest', json, ($checkedConvert) {
      final val = UpdateRoleRequest(
        name: $checkedConvert('name', (v) => v as String?),
        permissions: $checkedConvert(
          'permissions',
          (v) => _permissionsFromJson(v),
        ),
      );
      return val;
    });

Map<String, dynamic> _$UpdateRoleRequestToJson(UpdateRoleRequest instance) =>
    <String, dynamic>{
      'name': ?instance.name,
      'permissions': ?_permissionsToJson(instance.permissions),
    };
