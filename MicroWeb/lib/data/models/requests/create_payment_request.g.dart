// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_payment_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreatePaymentRequest _$CreatePaymentRequestFromJson(Map<String, dynamic> json) =>
    $checkedCreate('CreatePaymentRequest', json, ($checkedConvert) {
      final val = CreatePaymentRequest(
        beneficiaryName: $checkedConvert('beneficiaryName', (v) => v as String?),
        beneficiaryAccount: $checkedConvert('beneficiaryAccount', (v) => v as String?),
        fromAccount: $checkedConvert('fromAccount', (v) => v as String?),
        amount: $checkedConvert('amount', (v) => (v as num?)?.toDouble()),
        currency: $checkedConvert('currency', (v) => $enumDecodeNullable(_$CurrencyEnumMap, v)),
        details: $checkedConvert('details', (v) => v as String?),
      );
      return val;
    });

Map<String, dynamic> _$CreatePaymentRequestToJson(CreatePaymentRequest instance) => <String, dynamic>{
  'beneficiaryName': ?instance.beneficiaryName,
  'beneficiaryAccount': ?instance.beneficiaryAccount,
  'fromAccount': ?instance.fromAccount,
  'amount': ?(instance.amount == null ? null : (instance.amount! * 100).round()),
  'currency': ?_$CurrencyEnumMap[instance.currency],
  'details': ?instance.details,
};

const _$CurrencyEnumMap = {Currency.EUR: 0, Currency.USD: 1, Currency.PLN: 2, Currency.GBP: 3};
