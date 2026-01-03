# Test Coverage Summary

## OrderByParserTests (19 tests)

### Basic Functionality
- ? Parse_NullOrEmpty_ReturnsEmptyList
- ? Parse_SingleFieldNoDirection_ReturnsAscending
- ? Parse_SingleFieldAsc_ReturnsAscending
- ? Parse_SingleFieldDesc_ReturnsDescending

### Case Insensitivity - Keywords
- ? Parse_AscendingKeywords_CaseInsensitive (7 variations: ASC, asc, Asc, aSc, ascending, ASCENDING, Ascending)
- ? Parse_DescendingKeywords_CaseInsensitive (7 variations: DESC, desc, Desc, dEsC, descending, DESCENDING, Descending)

### Multiple Fields
- ? Parse_MultipleFields_ReturnsMultipleClauses
- ? Parse_MultipleFieldsWithExtraSpaces_HandlesCorrectly
- ? Parse_EmptyClausesIgnored_WithCommas

### Nested Properties
- ? Parse_NestedProperty_PreservesPropertyPath

### Property Name Case Preservation
- ? Parse_PropertyName_PreservesCase (4 variations: name, NAME, Name, nAmE)

### Error Handling
- ? Parse_InvalidDirection_ThrowsArgumentException

## ApplyOrderByTests (14 tests)

### Basic Functionality
- ? ApplyOrderBy_NullOrEmpty_ReturnsOriginal
- ? ApplyOrderBy_SingleFieldAscending_OrdersCorrectly
- ? ApplyOrderBy_SingleFieldDescending_OrdersCorrectly
- ? ApplyOrderBy_StringField_OrdersCorrectly

### Case Insensitivity - Property Names
- ? ApplyOrderBy_PropertyName_CaseInsensitive (4 variations: age, Age, AGE, aGe)
- ? ApplyOrderBy_NestedProperty_CaseInsensitive (5 variations)
- ? ApplyOrderBy_MixedCase_Everything_WorksCorrectly (3 variations)

### Multiple Fields
- ? ApplyOrderBy_MultipleFields_OrdersCorrectly
- ? ApplyOrderBy_ComplexMultipleFields_OrdersCorrectly

### Nested Properties
- ? ApplyOrderBy_NestedProperty_OrdersCorrectly

### Error Handling
- ? ApplyOrderBy_InvalidProperty_ThrowsException
- ? ApplyOrderBy_InvalidNestedProperty_ThrowsException

## Total: 33 Tests

All tests cover:
- ? Sort order keywords are case-insensitive (asc/ASC/Asc, desc/DESC/Desc, ascending, descending)
- ? Property names are case-insensitive (Name/name/NAME)
- ? Nested property paths are case-insensitive (Address.City/address.city)
- ? Multiple field ordering with ThenBy
- ? Error handling for invalid properties and directions
- ? Edge cases (null, empty, whitespace, extra commas)
