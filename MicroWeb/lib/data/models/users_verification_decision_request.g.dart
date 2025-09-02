// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'users_verification_decision_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UsersVerificationDecisionRequest _$UsersVerificationDecisionRequestFromJson(
  Map<String, dynamic> json,
) =>
    $checkedCreate('UsersVerificationDecisionRequest', json, ($checkedConvert) {
      final val = UsersVerificationDecisionRequest(
        targetId: $checkedConvert('targetId', (v) => v as String?),
        code: $checkedConvert('code', (v) => v as String?),
        accept: $checkedConvert('accept', (v) => v as bool?),
        newPassword: $checkedConvert('newPassword', (v) => v as String?),
      );
      return val;
    });

Map<String, dynamic> _$UsersVerificationDecisionRequestToJson(
  UsersVerificationDecisionRequest instance,
) => <String, dynamic>{
  'targetId': ?instance.targetId,
  'code': ?instance.code,
  'accept': ?instance.accept,
  'newPassword': ?instance.newPassword,
};
