// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'verification_decision_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

VerificationDecisionRequest _$VerificationDecisionRequestFromJson(
  Map<String, dynamic> json,
) => $checkedCreate('VerificationDecisionRequest', json, ($checkedConvert) {
  final val = VerificationDecisionRequest(
    code: $checkedConvert('code', (v) => v as String?),
    accept: $checkedConvert('accept', (v) => v as bool?),
  );
  return val;
});

Map<String, dynamic> _$VerificationDecisionRequestToJson(
  VerificationDecisionRequest instance,
) => <String, dynamic>{'code': ?instance.code, 'accept': ?instance.accept};
