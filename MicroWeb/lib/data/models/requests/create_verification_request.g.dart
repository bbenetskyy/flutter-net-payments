// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_verification_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateVerificationRequest _$CreateVerificationRequestFromJson(
  Map<String, dynamic> json,
) => $checkedCreate('CreateVerificationRequest', json, ($checkedConvert) {
  final val = CreateVerificationRequest(
    action: $checkedConvert(
      'action',
      (v) => $enumDecodeNullable(_$VerificationActionEnumMap, v),
    ),
    targetId: $checkedConvert('targetId', (v) => v as String?),
  );
  return val;
});

Map<String, dynamic> _$CreateVerificationRequestToJson(
  CreateVerificationRequest instance,
) => <String, dynamic>{
  'action': ?_$VerificationActionEnumMap[instance.action],
  'targetId': ?instance.targetId,
};

const _$VerificationActionEnumMap = {
  VerificationAction.NewUserCreated: 0,
  VerificationAction.UserAssignedToCard: 1,
  VerificationAction.CardPrinting: 2,
  VerificationAction.CardTermination: 3,
  VerificationAction.PaymentCreated: 4,
  VerificationAction.PaymentReverted: 5,
};
