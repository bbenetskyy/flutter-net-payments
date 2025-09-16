// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'role_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

RoleResponse _$RoleResponseFromJson(
  Map<String, dynamic> json,
) => $checkedCreate('RoleResponse', json, ($checkedConvert) {
  $checkKeys(
    json,
    requiredKeys: const [
      'id',
      'name',
      'permissions',
      'createdAt',
      'usersCount',
    ],
  );
  final val = RoleResponse(
    id: $checkedConvert('id', (v) => v as String),
    name: $checkedConvert('name', (v) => v as String),
    permissions: $checkedConvert('permissions', (v) => _permissionsFromJson(v)),
    createdAt: $checkedConvert('createdAt', (v) => DateTime.parse(v as String)),
    usersCount: $checkedConvert('usersCount', (v) => (v as num).toInt()),
  );
  return val;
});

Map<String, dynamic> _$RoleResponseToJson(RoleResponse instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'permissions': _permissionsToJson(instance.permissions),
      'createdAt': instance.createdAt.toIso8601String(),
      'usersCount': instance.usersCount,
    };
