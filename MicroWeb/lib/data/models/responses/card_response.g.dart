// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'card_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CardResponse _$CardResponseFromJson(
  Map<String, dynamic> json,
) => $checkedCreate('CardResponse', json, ($checkedConvert) {
  $checkKeys(
    json,
    requiredKeys: const [
      'id',
      'type',
      'name',
      'singleTransactionLimit',
      'monthlyLimit',
      'options',
      'printed',
      'terminated',
      'createdAt',
    ],
  );
  final val = CardResponse(
    id: $checkedConvert('id', (v) => v as String),
    type: $checkedConvert('type', (v) => $enumDecode(_$CardTypeEnumMap, v)),
    name: $checkedConvert('name', (v) => v as String),
    singleTransactionLimit: $checkedConvert(
      'singleTransactionLimit',
      (v) => (v as num).toInt(),
    ),
    monthlyLimit: $checkedConvert('monthlyLimit', (v) => (v as num).toInt()),
    assignedUserId: $checkedConvert('assignedUserId', (v) => v as String?),
    options: $checkedConvert(
      'options',
      (v) => $enumDecode(_$CardOptionsEnumMap, v),
    ),
    printed: $checkedConvert('printed', (v) => v as bool),
    terminated: $checkedConvert('terminated', (v) => v as bool),
    createdAt: $checkedConvert('createdAt', (v) => DateTime.parse(v as String)),
    updatedAt: $checkedConvert(
      'updatedAt',
      (v) => v == null ? null : DateTime.parse(v as String),
    ),
  );
  return val;
});

Map<String, dynamic> _$CardResponseToJson(CardResponse instance) =>
    <String, dynamic>{
      'id': instance.id,
      'type': _$CardTypeEnumMap[instance.type]!,
      'name': instance.name,
      'singleTransactionLimit': instance.singleTransactionLimit,
      'monthlyLimit': instance.monthlyLimit,
      'assignedUserId': ?instance.assignedUserId,
      'options': _$CardOptionsEnumMap[instance.options]!,
      'printed': instance.printed,
      'terminated': instance.terminated,
      'createdAt': instance.createdAt.toIso8601String(),
      'updatedAt': ?instance.updatedAt?.toIso8601String(),
    };

const _$CardTypeEnumMap = {CardType.Personal: 0, CardType.Shared: 1};

const _$CardOptionsEnumMap = {
  CardOptions.None: 0,
  CardOptions.ATM: 1,
  CardOptions.MagneticStripeReader: 2,
  CardOptions.Contactless: 4,
  CardOptions.OnlinePayments: 8,
  CardOptions.AllowChangingSettings: 16,
  CardOptions.AllowPlasticOrder: 32,
};
