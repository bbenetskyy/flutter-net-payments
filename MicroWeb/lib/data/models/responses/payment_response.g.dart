// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'payment_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

PaymentResponse _$PaymentResponseFromJson(Map<String, dynamic> json) =>
    $checkedCreate('PaymentResponse', json, ($checkedConvert) {
      $checkKeys(
        json,
        requiredKeys: const [
          'id',
          'userId',
          'beneficiaryName',
          'beneficiaryAccount',
          'fromAccount',
          'amount',
          'currency',
          'fromCurrency',
          'status',
          'createdAt',
        ],
      );
      final val = PaymentResponse(
        id: $checkedConvert('id', (v) => v as String),
        userId: $checkedConvert('userId', (v) => v as String),
        beneficiaryName: $checkedConvert('beneficiaryName', (v) => v as String),
        beneficiaryAccount: $checkedConvert('beneficiaryAccount', (v) => v as String),
        beneficiaryId: $checkedConvert('beneficiaryId', (v) => v as String?),
        beneficiaryAccountId: $checkedConvert('beneficiaryAccountId', (v) => v as String?),
        fromAccount: $checkedConvert('fromAccount', (v) => v as String),
        amount: $checkedConvert('amount', (v) => (v as num).toDouble() / 100.0),
        currency: $checkedConvert('currency', (v) => v as String),
        fromCurrency: $checkedConvert('fromCurrency', (v) => v as String),
        details: $checkedConvert('details', (v) => v as String?),
        status: $checkedConvert('status', (v) => v as String),
        createdAt: $checkedConvert('createdAt', (v) => DateTime.parse(v as String)),
        updatedAt: $checkedConvert('updatedAt', (v) => v == null ? null : DateTime.parse(v as String)),
      );
      return val;
    });

Map<String, dynamic> _$PaymentResponseToJson(PaymentResponse instance) => <String, dynamic>{
  'id': instance.id,
  'userId': instance.userId,
  'beneficiaryName': instance.beneficiaryName,
  'beneficiaryAccount': instance.beneficiaryAccount,
  'beneficiaryId': ?instance.beneficiaryId,
  'beneficiaryAccountId': ?instance.beneficiaryAccountId,
  'fromAccount': instance.fromAccount,
  'amount': instance.amount,
  'currency': instance.currency,
  'fromCurrency': instance.fromCurrency,
  'details': ?instance.details,
  'status': instance.status,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': ?instance.updatedAt?.toIso8601String(),
};
