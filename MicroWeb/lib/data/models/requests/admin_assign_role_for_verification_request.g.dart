// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'admin_assign_role_for_verification_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AdminAssignRoleForVerificationRequest
_$AdminAssignRoleForVerificationRequestFromJson(Map<String, dynamic> json) =>
    $checkedCreate('AdminAssignRoleForVerificationRequest', json, (
      $checkedConvert,
    ) {
      final val = AdminAssignRoleForVerificationRequest(
        desiredRoleId: $checkedConvert('desiredRoleId', (v) => v as String?),
      );
      return val;
    });

Map<String, dynamic> _$AdminAssignRoleForVerificationRequestToJson(
  AdminAssignRoleForVerificationRequest instance,
) => <String, dynamic>{'desiredRoleId': ?instance.desiredRoleId};
